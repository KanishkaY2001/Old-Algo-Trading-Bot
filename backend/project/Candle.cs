namespace TradingBot
{
    public class Candle
    {
        /* General Candle Data */
        public long unix { get; set; }
        public decimal open { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal close { get; set; }

        /* Indicator Profiles */
        public GenData genData { get; set; } = new GenData();
        public HiLoData hiloData { get; set; } = new HiLoData();
        public MacdSData mcdSData { get; set; } = new MacdSData();
        public RsiData rsiData { get; set; } = new RsiData();

        public Candle(int ux, decimal op, decimal hi, decimal lo, decimal cl)
        {
            unix = ux;
            open = op;
            high = hi;
            low = lo;
            close = cl;
        }
    }
}