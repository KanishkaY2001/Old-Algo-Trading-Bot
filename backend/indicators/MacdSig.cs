namespace TradingBot
{
    public class MacdSigOpt
    {
        public decimal? macdSum { get; set; } = 0;
        public decimal? fastSum { get; set; } = 0;
        public decimal? slowSum { get; set; } = 0;
        public List<decimal> smaMacd { get; set; } = new List<decimal>();
        public List<decimal> smaFast { get; set; } = new List<decimal>();
        public List<decimal> smaSlow { get; set; } = new List<decimal>();

        public int fastLength { get; set; }
        public int slowLength { get; set; }
        public int signalLength { get; set; }
        public MacdSigOpt(int fl, int sl, int sig)
        {
            fastLength = fl;
            slowLength = sl;
            signalLength = sig;
        }
    }

    public class MacdSig
    {
        private static decimal? GetEma(Project project, string type, int len, decimal? emaPrev)
        {
            var data = project.data;
            var mdp = project.macdSOpt;

            if (data.Count < len) 
                return null;
            
            Candle candle = data.Last();
            decimal multiplier = (2m / (len + 1));
            decimal? op1 = PineSim.Iff(type.Equals("fast"), mdp.fastSum / len, mdp.slowSum / len);
            decimal? op2 = candle.close * multiplier + emaPrev * (1 - multiplier);          
            return emaPrev == null? op1 : op2;
        }

        private static decimal? getSig(int len, Project project)
        {
            var data = project.data;
            int min = Math.Min(len, data.Count);
            var smaMacd = project.macdSOpt.smaMacd;
            decimal? macd = data.Last().macd;

            if (macd == null)
                return null;

            if (smaMacd.Count >= len)
            {
                project.macdSOpt.macdSum -= smaMacd[0];
                smaMacd.RemoveAt(0);
            }

            project.macdSOpt.macdSum += (decimal)macd;
            smaMacd.Add((decimal)macd);
            return project.macdSOpt.macdSum / len;
        }

        public static void ApplyIndicator(Project project)
        {
            var data = project.data;
            if (data.Count == 0)
                return;

            Candle target = data.Last();
            var prevTarget = data.Count > 1? data[data.Count - 2] : null;

            var p = project.macdSOpt;
            int fast = project.macdSOpt.fastLength;
            int slow = project.macdSOpt.slowLength;
            int sig = project.macdSOpt.signalLength;

            /* Store Fast SMA Data */
            if (p.smaFast.Count >= fast)
            {
                p.fastSum -= p.smaFast[0];
                p.smaFast.RemoveAt(0);
            }
            p.fastSum += target.close;
            p.smaFast.Add(target.close);
            

            /* Store Slow SMA Data */
            if (p.smaSlow.Count >= slow)
            {
                p.slowSum -= p.smaSlow[0];
                p.smaSlow.RemoveAt(0);
            }
            p.slowSum += target.close;
            p.smaSlow.Add(target.close);

            var prevF = data.Count - 2 >= 0? data[data.Count - 2].fast : null;
            var prevS = data.Count - 2 >= 0? data[data.Count - 2].slow : null;
            target.fast = GetEma(project, "fast", fast, prevF);
            target.slow = GetEma(project, "slow", slow, prevS);
            
            target.macd = target.fast - target.slow;
            target.signal = getSig(sig, project);

            if (data.Count < slow + sig - 1)
                target.signal = null;

            target.hist = target.macd - target.signal;
            target.macDecision = MakeDecision(project, target);
        }

        private static string MakeDecision(Project project, Candle candle)
        {
            var curr = candle.hist;
            char color = candle.macd >= candle.signal ? 'g' : 'r';
            if (project.data.Count < 2)
                return "idle";

            var prev = project.data[project.data.Count - 2].hist;
            bool cross = (prev < 0 && curr >= 0) || (prev >= 0 && curr < 0);

            if (color.Equals('g') && cross)
                return "buy";

            if (color.Equals('r') && cross)
                return "sell";

            return "idle";
        }
    }

    /* Trade Responsibility Chain */
    public class TradeDecMacd : TaskHandler
    {
        public override object? HandleTask(Project project)
        {
            var data = project.data;
            var candle = data.Last();

            /* MacdSignal Indicator Dependency */
            if (candle.signal == null)
                candle.finalDecision = "idle";

            string prevDec = data[data.Count - 2].finalDecision;
            string macdDec = candle.macDecision;

            if (!prevDec.Equals("hodl") && macdDec.Equals("buy"))
                project.NormalBuy();

            else if ((prevDec.Equals("hodl") || prevDec.Equals("buy")) && macdDec.Equals("sell"))
                project.NormalSell();

            return base.HandleTask(project);
        }
    }
}