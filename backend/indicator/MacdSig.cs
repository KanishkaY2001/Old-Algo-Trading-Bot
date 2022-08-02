namespace TradingBot
{
    public class MacdSData
    {
        public decimal? signal { get; set; }
        public decimal? macd { get; set; }
        public decimal? hist { get; set; }
        public decimal? fast { get; set; }
        public decimal? slow { get; set; }
        public string decision { get; set;} = "-";
    }
}