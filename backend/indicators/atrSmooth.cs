namespace TradingBot
{
    public class atrSmoothOpt
    {
        public int period { get; set; }
        public decimal multi { get; set; }
        public decimal? prevSum { get; set; } = null;
        public List<decimal> ranges { get; set; }
        public List<decimal> closes { get; set; }
        public decimal trailingStop { get; set; }
        public int pos { get; set; }
        public bool isLong { get; set; } = false;
        public bool isShort { get; set; } = false;

        public atrSmoothOpt(int _p, decimal _m)
        {
            period = _p;
            multi = _m;
            ranges = new List<decimal>();
            closes = new List<decimal>();
        }
    }


    public class atrSmooth
    {

        private static void getRma(Project project, decimal range)
        {
            var opt = project.atrSmoothOpt;
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
            var opt = project.atrSmoothOpt;
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

        public static decimal Iff(bool cond, decimal _then, decimal _else)
        {
            return cond ? _then : _else;
        }

        public static void ApplyIndicator(Project project)
        {
            var data = project.data;
            var opt = project.atrSmoothOpt;
            getAtr(project);
            
            var candle = data.Last();
            



            /*var latestClose = candle.close;
            opt.closes.Add(latestClose);
            if (opt.closes.Count > opt.period)
            {
                opt.closes.RemoveAt(0);
            }*/


            if (data.Count < opt.period)
                return;

            decimal close = candle.close;
            decimal close1 = data[data.Count() - 2].close;
            decimal nLoss = (decimal)opt.prevSum! * opt.multi;

            decimal prevTrailingStop = opt.trailingStop;
            opt.trailingStop = Iff(close > prevTrailingStop && close1 > prevTrailingStop, Math.Max(prevTrailingStop, close - nLoss), 
                Iff(close < prevTrailingStop && close1 < prevTrailingStop, Math.Min(prevTrailingStop, close + nLoss),
                    Iff(close > prevTrailingStop, close - nLoss, close + nLoss)));

            //Console.WriteLine($"{Helper.UnixToDate(candle.unix)} : {opt.trailingStop}");

            int prevPos = opt.pos;
            if (close1 < prevTrailingStop && close > prevTrailingStop) opt.pos = 1;
            else if (close1 > prevTrailingStop && close < prevTrailingStop) opt.pos = -1;
            else opt.pos = prevPos;

            bool LONG = opt.isLong == false && opt.pos == 1;
            bool SHORT = opt.isShort == false && opt.pos == -1;
            
            if (LONG)
            {
                candle.smoothDecision = "buy";
                //Console.WriteLine($"{Helper.UnixToDate(candle.unix)} : BUY");
                opt.isLong = true;
                opt.isShort = false;
            }
            
            if (SHORT)
            {
                candle.smoothDecision = "sell";
                //Console.WriteLine($"{Helper.UnixToDate(candle.unix)} : SELL");
                opt.isLong = false;
                opt.isShort = true;
            }
        }
    }
}