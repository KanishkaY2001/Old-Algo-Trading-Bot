namespace TradingBot
{
    public sealed class Manager
    {
        private static readonly Lazy<Manager> manager = new Lazy<Manager>(() => new Manager());
        public static Manager Global { get { return manager.Value; } }

        public Dictionary<string, Project> projects { get; set; } = new Dictionary<string, Project>();
        public Dictionary<string, IMarket> markets { get; set; } = new Dictionary<string, IMarket> 
        {
            {"KuCoin", new KuCoin()}
        };

        public TaskHandler tradeDecHead { get; } = new TradeDecHead();
        public TaskHandler tradeDecHiLo { get; } = new TradeDecHiLo();
        public TaskHandler tradeDecMacdS { get; } = new TradeDecMacd();
        public TaskHandler tradeDecTail { get; } = new TradeDecTail();

        public Manager()
        {
            tradeDecHead.SetNext(tradeDecHiLo).SetNext(tradeDecTail);
        }


        public async Task<bool> AddSecurityToMarket(string market, string security)
        {
            return await markets[market].AddSecurity(security);
        }
    }
}