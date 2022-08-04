namespace TradingBot
{
    public sealed class Manager
    {
        private static readonly Lazy<Manager> manager = new Lazy<Manager>(() => new Manager());
        public static Manager Global { get { return manager.Value; } }

        public Dictionary<string, Project> projects { get; set; } = new Dictionary<string, Project>();

        public TaskHandler tradeDecHead { get; } = new TradeDecHead();
        public TaskHandler tradeDecHiLo { get; } = new TradeDecHiLo();
        public TaskHandler tradeDecTail { get; } = new TradeDecTail();

        public Manager()
        {
            tradeDecHead.SetNext(tradeDecHiLo).SetNext(tradeDecTail);;
        }
    
    }
}