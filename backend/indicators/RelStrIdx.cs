namespace TradingBot
{
    public class RsiOpt
    {
        public int rsiLen { get; set; }
        public int maLen { get; set; }
        public RsiOpt(int rl, int ml)
        {
            rsiLen = rl;
            maLen = ml;
        }

        public decimal up { get; set; }
        public decimal down { get; set; }
        public decimal rsiSum { get; set; }
        public Queue<decimal?> rsiQ = new Queue<decimal?>();
    }

    public class RelStrIdx
    {
        private static void GetRsiSma(Project project, decimal? newMa, int len)
        {
            var info = project.rsiOpt;
            info.rsiQ.Enqueue(newMa / len);
            info.rsiSum += (decimal)info.rsiQ.Last()!;

            if (info.rsiQ.Count >= 14)
            {
                project.data.Last().rsiMA = info.rsiSum;
                info.rsiSum -= (decimal)info.rsiQ.Dequeue()!;
            }
        }

        private static decimal GetSma(List<Candle> data, int len, bool up)
        {
            decimal sum = 0;
            for (int i = 1; i < len + 1; ++i)
            {
                if (up)
                    sum += Math.Max(data[i].close - data[i - 1].close, 0) / len;
                else
                    sum += -Math.Min(data[i].close - data[i - 1].close, 0) / len;
            }
            return sum;
        }

        private static decimal GetRma(RsiOpt opt, decimal src, int len, bool up)
        {     
            decimal alpha = 1m / len;
            if (up)
            {
                opt.up = alpha * src + (1 - alpha) * PineSim.Nz(opt.up);
                return opt.up;
            }
            else
            {
                opt.down = alpha * src + (1 - alpha) * PineSim.Nz(opt.down);
                return opt.down;
            }
        }

        public static void ApplyIndicator(Project project)
        {  
            var data = project.data;
            var profile = project.rsiOpt;
            int len = profile.rsiLen;
            if (data.Count < len + 1)
                return;
            
            
            if (profile.down == 0)
            {
                profile.up = GetSma(data, len, true);
                profile.down = GetSma(data, len, false);
            }
            else
            {
                decimal change = data.Last().close - data[data.Count - 2].close;
                GetRma(profile, Math.Max(change, 0), len, true);
                GetRma(profile, -Math.Min(change, 0), len, false);
            }

            Candle candle = data.Last();
            candle.rsi = profile.down == 0 ? 100 : profile.up == 0 ? 0 : 100 - (100 / (1 + profile.up / profile.down));
            GetRsiSma(project, candle.rsi, len);
        }
    }
}