namespace TradingBot
{
    public class ChandelierOpt
    {
        public int period { get; set; }
        public decimal multi { get; set; }
        public decimal? prevSum { get; set; } = null;
        public List<decimal> ranges { get; set; }
        public decimal longStop { get; set; }
        public decimal shortStop { get; set; }
        public List<decimal> closes { get; set; }
        public decimal lowest { get; set; }
        public decimal highest { get; set; }
        public int dir { get; set; } = 1;
        
        public ChandelierOpt(int _p, decimal _m)
        {
            period = _p;
            multi = _m;
            ranges = new List<decimal>();
            closes = new List<decimal>();
        }
    }


    public class Chandelier
    {
        private static void getRma(Project project, decimal range)
        {
            var opt = project.chandalierOpt;
            decimal alpha = (1m / opt.period);
            
            if (opt.prevSum == null)
            {
                decimal sum = 0;
                for (int i = 0; i < opt.ranges.Count; ++i)
                {
                    sum = sum + opt.ranges[i] / opt.period;
                }
                opt.prevSum = sum;
                return;
            }
            opt.prevSum = alpha * range + (1 - alpha) * (opt.prevSum ?? 0);
        }

        private static void getAtr(Project project)
        {
            var opt = project.chandalierOpt;
            decimal range = 0;
            var data = project.data;
            var curr = data[data.Count - 1];
            decimal high = curr.high;
            decimal low = curr.low;

            if (data.Count < 2)
            {
                range = curr.high - curr.low;
            }
            else
            {
                decimal prevClose = data[data.Count - 2].close;
                decimal x = Math.Max(high - low, Math.Abs(high - prevClose));
                decimal y = Math.Abs(low - prevClose);
                range = Math.Max(x, y);
            }

            if (project.data.Count <= opt.period)
                opt.ranges.Add(range);

            if (project.data.Count >= opt.period)
                getRma(project, range);
        }

        public static void ApplyIndicator(Project project)
        {
            var data = project.data;
            var opt = project.chandalierOpt;
            getAtr(project);
        
            var candle = data[data.Count - 1];
            var latestClose = candle.close;
            opt.closes.Add(latestClose);
            if (opt.closes.Count > opt.period)
            {
                opt.closes.RemoveAt(0);
            }
            opt.highest = opt.closes.Max();
            opt.lowest = opt.closes.Min();

            if (data.Count < opt.period)
                return;

            decimal atr = (decimal)opt.prevSum! * opt.multi;

            var prevCandle = data[data.Count - 2];
            var curCandle = data[data.Count - 1];

            var longStopPrev = opt.longStop;
            opt.longStop = opt.highest - atr;
            opt.longStop = prevCandle.close > longStopPrev ? Math.Max(opt.longStop, longStopPrev) : opt.longStop;
            
            var shortStopPrev = opt.shortStop;
            opt.shortStop = opt.lowest + atr;
            opt.shortStop = prevCandle.close < shortStopPrev ? Math.Min(opt.shortStop, shortStopPrev) : opt.shortStop;

            var prevDir = opt.dir;
            opt.dir = latestClose > shortStopPrev ? 1 : latestClose < longStopPrev ? -1 : opt.dir;
            //Console.WriteLine();

            //Console.WriteLine($"{Helper.UnixToDate(data.Last().unix)} | {opt.dir}");

            candle.chandDecision = MakeDecision(opt.dir, prevDir, candle);
        }

        private static string MakeDecision(int dir, int prevDir, Candle candle)
        {
            if (dir == 1 && prevDir == -1)
            {
                return "buy";
            }
            else if (dir == -1 && prevDir == 1)
            {
                return "sell";
            }
            return "-";
        }
    }


    public class TradeDecChand : TaskHandler
    {
        public override object? HandleTask(Project project)
        {
            var data = project.data;
            var candle = data.Last();
            string prevDec = data[data.Count - 2].finalDecision;

            /*
            if (!prevDec.Equals("hodl") && candle.chandDecision.Equals("buy"))
                project.NormalBuy();

            else if ((prevDec.Equals("hodl") || prevDec.Equals("buy")) && candle.chandDecision.Equals("sell"))
                project.NormalSell();
            */

            if (candle.chandDecision.Equals("buy"))
                project.NormalBuy();

            else if (candle.chandDecision.Equals("sell"))
                project.NormalSell();

            return base.HandleTask(project);
        }
    }
}