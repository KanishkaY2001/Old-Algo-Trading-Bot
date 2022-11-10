using KuCoinFiles;

namespace TradingBot
{
    public sealed class Manager
    {
        private static readonly Lazy<Manager> manager = new Lazy<Manager>(() => new Manager());
        public static Manager Global { get { return manager.Value; } }
        public string filePath { get; set; } = "./backend/project/output/";
        public Dictionary<string, Project> projects { get; set; } = new Dictionary<string, Project>();
        public Dictionary<string, string> outputs { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, IMarket> markets { get; set; } = new Dictionary<string, IMarket> 
        {
            { "KuCoin" , new KuCoin() }
        };

        public TaskHandler tradeDecHead { get; } = new TradeDecHead();
        public TaskHandler tradeDecHiLo { get; } = new TradeDecHiLo();
        public TaskHandler tradeDecMacdS { get; } = new TradeDecMacd();
        public TaskHandler tradeDecChand { get; } = new TradeDecChand();
        public TaskHandler tradeDecTail { get; } = new TradeDecTail();
        public bool canPlaceOrder { get; set; } = false;

        public Manager()
        {
            tradeDecHead.SetNext(tradeDecChand).SetNext(tradeDecTail);
        }


        public async Task<bool> AddSecurityToMarket(string market, string p1, string p2)
        {
            if (!markets.ContainsKey(market))
                return false;

            return await markets[market].AddSecurity(p1, p2);
        }


        public bool AddProject(string name, string market, string p1, string p2, string period)
        {
            if (projects.ContainsKey(name))
                return false;

            var p = new Portfolio(p1, p2, 0, 100, 100, 0.001m, 0.001m);
            string cpCode = $"{p1}{markets[market].code}{p2}";
            string output = $"{filePath}{name}_{market}_{p1}_{p2}.csv";
            
            // Project with given name already exists with the market, and coin-pair
            if (outputs.TryGetValue(output, out string? _name))
                return false;

            var project = new Project(tradeDecHead, p, market, cpCode, name, period);
            outputs.Add(name, output);
            projects.Add(name, project);

            return true;
        }

        // If no data exists within the project, fill data with Market Coin-Pair data
        public bool TryProjectFill(Project project, string _m, string _cp, string _out)
        {
            if (project.data.Count != 0)
                return false;
            
            var candles = markets[_m].securities[_cp];
            for (int i = 0; i < candles.Count; ++i)
            {
                project.ProcessCandle(candles[i], false);
            }

            ProcessFile.ProcessAll(_out, project);
            return true;
        }

        // Add new Coin-Pair data every X time from the market
        public void UpdateProjects(Candle? candle, string _m, string _cp)
        {
            foreach (var project in projects.Values)
            {
                if (!project.market.Equals(_m))
                    continue;

                if (!project.cpCode.Equals(_cp))
                    continue;

                string output = outputs[project.name];
                if (TryProjectFill(project, _m, _cp, output))
                    continue;
                
                bool ES = project.EmergencySell();
                if (candle == null)
                {
                    if (ES && canPlaceOrder)
                    {
                        markets[_m].PlaceOrder("sell", _cp);
                    }   
                    continue;
                }

                if (ES || project.ProcessCandle(candle, true))
                {
                    
                    if (canPlaceOrder)
                        markets[_m].PlaceOrder(candle.finalDecision, _cp);
                    ProcessFile.ProcessNext(output, project);
                    //Task saveToFile = new Task( () => ProcessFile.ProcessNext(output, project) );
                    //saveToFile.Start();
                }
                    
            }
        }
    }
}