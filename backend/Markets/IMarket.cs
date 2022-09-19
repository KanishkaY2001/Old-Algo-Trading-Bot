namespace TradingBot
{
    public interface IMarket
    {
        public int storageAmount { get; set; }
        public string uri { get; set; }
        public string[] uriParams { get; set; }
        public Dictionary<string,List<Candle>> securities { get; set; }
        public void UpdateManager(Candle candle, string market);
        public Task<bool> AddSecurity(string security);
    }
}