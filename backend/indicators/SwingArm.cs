namespace TradingBot
{
    public class SwingArmOpt
    {
        public int period { get; set; }
        public int factor { get; set; }
        public List<decimal> smas { get; set; } = new List<decimal>();
        public decimal smaSum { get; set; }
        public decimal hilo { get; set; }
        public bool mod { get; set; }
        public decimal wildMa { get; set; } = 0m;
        public decimal trendUp { get; set; }
        public decimal trendDown { get; set; }
        public decimal trend { get; set; }
        public decimal ex { get; set; } = 0m;

        public SwingArmOpt(int _p, int _f, bool _m)
        {
            period = _p;
            factor = _f;
            mod = _m;
        }
    }

    public class SwingArm
    {
        private static decimal Max(decimal val1, decimal val2, decimal val3)
        {
            var max = Math.Max(val1, val2);
            max = Math.Max(max, val3);
            return max;
        }

        public static void ApplyIndicator(Project project)
        {
            var data = project.data;
            var count = data.Count();
            var target = data.Last();
            var opt = project.swingArmOpt;
           
            opt.smas.Add(target.high - target.low);
            opt.smaSum += opt.smas.Last();
            if (count > opt.period)
            {
                opt.smaSum -= opt.smas.First();
                opt.smas.RemoveAt(0);

            }

            opt.hilo = Math.Min(target.high - target.low, 1.5m * (opt.smaSum / opt.period));

            if (count < 2) return;
            var prev = data[count - 2];

            var href = target.low <= prev.high ? target.high - prev.close : (target.high - prev.close) - 0.5m * (target.low - prev.high);
            var lref = target.high >= prev.low ? prev.close - target.low : (prev.close - target.low) - 0.5m * (prev.low - target.high);

            //Console.WriteLine($"loss: {lref}");
            var tr = opt.mod ? Math.Max(Math.Max(opt.hilo, href), lref) : Math.Max(Math.Max(target.high - target.low, Math.Abs(target.high - prev.close)), Math.Abs(target.low - prev.close));
            //Console.WriteLine($"loss: {Max(opt.hilo, href, lref)}");

            var prevMa = opt.wildMa;
            opt.wildMa = prevMa + ((tr - prevMa) / opt.period);
            var loss = opt.factor * opt.wildMa;

            var up = target.close - loss;
            var down = target.close + loss;
            //Console.WriteLine($"up: {up}  ||  down: {down}");

            if (opt.trend == 0)
            {
                opt.trendUp = up;
                opt.trendDown = down;
                opt.trend = 1;
            }

            opt.trendUp = prev.close > opt.trendUp ? Math.Max(up, opt.trendUp) : up;
            opt.trendDown = prev.close < opt.trendDown ? Math.Min(down, opt.trendDown) : down;

            var nextTrend = target.close > opt.trendDown ? 1 : target.close < opt.trendUp ? -1 : opt.trend;
            //var trail = opt.trend == 1? opt.trendUp : opt.trendDown;

            if (nextTrend != opt.trend)
            {
                if (nextTrend == 1) target.swingDecision = "buy";
                else target.swingDecision = "sell";
            }
            else
            {
                target.swingDecision = "-";
            }
            opt.trend = nextTrend;
            //Console.WriteLine($"candles: {Helper.UnixToDate(target.unix)}  ||  hilo: {target.swingDecision}");
        }
    }

    public class TradeDecSwing : TaskHandler
    {
        public override object? HandleTask(Project project)
        {
            var data = project.data;
            var candle = data.Last();
            Console.WriteLine($"HANDLING TASK: {candle.swingDecision}");
            if (candle.swingDecision.Equals("buy"))
            {
                Console.WriteLine("Buy");
                project.NormalBuy();
            }

            else if (candle.swingDecision.Equals("sell"))
            {
                Console.WriteLine("Sell");
                project.NormalSell();
            }
                

            return base.HandleTask(project);
        }
    }
}