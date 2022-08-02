namespace TradingBot
{
    public class HiLoData
    {
        public decimal? hl { get; set; } // Trend direction
        public decimal? zz { get; set; } // Zig zag curve
        public string hilo { get; set; } = "-";
        public string decision { get; set; } = "-";
    }
}