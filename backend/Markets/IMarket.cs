namespace TradingBot
{
    public interface IMarket
    {
        public int storageAmount { get; set; }
        public string[] uriParams { get; set; }
        public List<string> securities { get; set; }
        public Dictionary<string, Order> orders { get; set; }
        public Task<string> AddSecurity(Project project);
        public void SocketMessage(string msg);
        public void PlaceOrder(Project project, string decision, bool newPos);
        public Candle CreateCandle(List<string> candleInfo);
        public string GetUri(int period, int duration, string security);
        public Task<List<Candle>> GetDataFill(string uri, int expectedCount);
    }
}