using System.Text.RegularExpressions;

namespace TradingBot
{
    public class HighLow
    {
        private static volatile Dictionary<string, Regex> buyPatterns = new Dictionary<string, Regex>
        {
            { "e1" , new Regex("(ll )(- )*(lh )+((- )|(lh ))*(hl$)", RegexOptions.Compiled) }, /* LL -> LH -> HL */
            { "e2" , new Regex("(hl )(- )*(lh )+((- )|(lh ))*(hl$)", RegexOptions.Compiled) }, /* HL -> LH -L HL */
            { "e3" , new Regex("(ll )(- )*(hh )+((- )|(hh ))*(hl$)", RegexOptions.Compiled) }  /* LL -> HH -> HL */
        };

        /* Sell Patterns */
        private static volatile Dictionary<string, Regex> sellPatterns = new Dictionary<string, Regex>
        {
            { "x1" , new Regex("ll$", RegexOptions.Compiled) }, /* LL */
        };

        private static decimal[] FindPrevious(Project p, Candle candle, decimal? ehl)
        {
            decimal[] previousArr = new decimal[4];
            int arrIdx = 0;
            int count = p.data.Count;
            for (int i = 1; i <= count; ++i)
            {
                Candle c = p.data[count - i];

                if (count > i && c.hl == ehl && c.zz != null)
                {
                    previousArr[arrIdx] = (decimal)c.zz!;
                    if (++arrIdx == 4)
                        break;

                    var alt = PineSim.Iff(candle.hl == 1, -1, 1)!;
                    ehl = arrIdx % 2 == 0? (decimal)alt : candle.hl;
                }
            }
            return previousArr;
        }

        public static void ApplyIndicator(Project p)
        {
            int idx = p.data.Count - 1;
            if (idx < 0 || idx < p.rightBars + p.leftBars)
                return;

            Candle candle = p.data[idx];
            Candle target = p.data[idx - p.rightBars];

            decimal? ph = p.PivotHigh();
            decimal? pl = p.PivotLow();
            
            decimal? prevHl = p.ValueWhen("hl","hl",1);
            decimal? prevZz = p.ValueWhen("zz","zz",1);

            target.hl = PineSim.Iff(ph != null, 1, PineSim.Iff(pl != null, -1, null));
            target.zz = PineSim.Iff(ph != null, ph, PineSim.Iff(pl != null, pl, null));
            target.zz = PineSim.Iff(pl != null && target.hl== -1 && prevHl == -1 && pl > prevZz, null, target.zz);
            target.zz = PineSim.Iff(ph != null && target.hl== 1 && prevHl == 1  && ph < prevZz, null, target.zz);

            target.hl= PineSim.Iff(target.hl== -1 && prevHl == 1 && target.zz > prevZz, null, target.hl);
            target.hl= PineSim.Iff(target.hl== 1 && prevHl == -1 && target.zz < prevZz, null, target.hl);
            target.zz = PineSim.Iff(target.hl== null, null, target.zz);

            decimal[] previous = FindPrevious(p, target, PineSim.Iff(target.hl == 1, -1, 1));

            string hl = "-";
            if (target.hl != null && target.zz != null)
            {   
                
                decimal a = (decimal)target.zz;
                decimal b = previous[0];
                decimal c = previous[1];
                decimal d = previous[2];
                decimal e = previous[3];

                hl = (a > b && a > c && c > b && c > d)? "hh" : hl;
                hl = (a < b && a < c && c < b && c < d)? "ll" : hl;
                hl = ((a >= c && (b > c && b > d && d > c && d > e)) || (a < b && a > c && b < d))? "hl" : hl;
                hl = ((a <= c && (b < c && b < d && d < c && d < e)) || (a > b && a < c && b > d))? "lh" : hl;
                
                target.hilo = hl;
            }
            
            candle.hiloDecision = MakeDecision(p, hl, candle);
            //Console.WriteLine($"unix: {Helper.UnixToDate(target.unix)} | hilo : {target.hilo}");
        }


        private static bool CheckPattern(List<string> data, Dictionary<string, Regex> patterns, Candle candle)
        {
            string sample = string.Join(" ", data);
            foreach (Regex pattern in patterns.Values)
            {
                Match match = pattern.Match(sample);
                if (match.Length != 0)
                    return true;
            }
            return false;
        }


        private static string MakeDecision(Project project, string hl, Candle candle)
        {
            List<string> data = project.HiLoPattern;
            if (data.Count > project.maxPattern) data.RemoveAt(0);
            data.Add(hl);

            if (CheckPattern(data, buyPatterns, candle))
                return "buy";

            if (CheckPattern(data, sellPatterns, candle))
                return "sell";

            return "idle";
        }
    }

    /* Trade Responsibility Chain */
    public class TradeDecHiLo : TaskHandler
    {
        public override object? HandleTask(Project project)
        {
            var data = project.data;
            var candle = data.Last();
            string prevDec = data[data.Count - 2].finalDecision;

            /* MacdSignal Indicator Dependency */
            if (candle.signal == null)
                candle.hiloDecision = "idle";

            int right = project.rightBars;
            var ptrn = data[data.Count - 1 - (data.Count >= right + 1? right : 0)];
            bool red = ptrn.macd >= ptrn.signal ? false : true;

            if (!prevDec.Equals("hodl") && candle.hiloDecision.Equals("buy"))
                project.NormalBuy();

            else if (red && (prevDec.Equals("hodl") || prevDec.Equals("buy")) && candle.hiloDecision.Equals("sell"))
                project.NormalSell();

            else if (prevDec.Equals("hodl") || prevDec.Equals("buy"))
                project.EmergencySell();

            return base.HandleTask(project);
        }
    }
}