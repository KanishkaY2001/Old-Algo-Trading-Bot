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
            if (!markets.ContainsKey(market))
                return false;

            return await markets[market].AddSecurity(security);
        }


        public void AddProject(string name, Project project)
        {
            if (projects.ContainsKey(name))
                return;
            
            projects.Add(name, project);
        }


        public void UpdateProjects(Candle candle, string market)
        {
            foreach (var project in projects.Values)
            {
                if (!project.market.Equals(market))
                    continue;

                project.ProcessCandle(candle);
            }
        }
    }
}