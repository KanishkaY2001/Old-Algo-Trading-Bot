namespace TradingBot
{
    public class StochRsiOpt
    {
        public int smaLen { get; set; }
        public int lenStoch { get; set; }
        public decimal kSum { get; set; } = 0;
        public decimal dSum { get; set; } = 0;
        public Queue<decimal> kQ { get; set; }
        public Queue<decimal> dQ { get; set; }
        public Queue<decimal> rsiQ { get; set; }

        public StochRsiOpt(int lenSt, int sLen)
        {
            lenStoch = lenSt;
            smaLen = sLen;
            kQ = new Queue<decimal>();
            dQ = new Queue<decimal>();
            rsiQ = new Queue<decimal>();
        }
    }

    public class StochRsi
    {
        public static void ApplyIndicator(Project project)
        {
            var data = project.data;
            var target = data.Last();

            if (target.rsi == null)
                return;

            var opt = project.stochRsiOpt;
            int len = opt.lenStoch;
            decimal rsi = (decimal)target.rsi;
            
            opt.rsiQ.Enqueue(rsi);
            if (opt.rsiQ.Count == opt.lenStoch + 1)
                opt.rsiQ.Dequeue();

            decimal low = opt.rsiQ.Min();
            var stoch = opt.rsiQ.Count > 2 ? 100 * (rsi - low) / (opt.rsiQ.Max() - low) : rsi;

            opt.kQ.Enqueue(stoch);
            opt.kSum += stoch;
            if (opt.kQ.Count == opt.smaLen + 1)
                opt.kSum -= opt.kQ.Dequeue();
            
            decimal k = opt.kSum / opt.kQ.Count;
            
            opt.dQ.Enqueue(k);
            opt.dSum += k;
            if (opt.dQ.Count == opt.smaLen + 1)
                opt.dSum -= opt.dQ.Dequeue();

            target.K = k;
            target.D = opt.dSum / opt.dQ.Count;
        }
    }
}