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
        public TaskHandler tradeDecSmooth { get; } = new TradeDecSmooth();
        public TaskHandler tradeDecSwing { get; } = new TradeDecSwing();
        public TaskHandler tradeDecTail { get; } = new TradeDecTail();
        
        public bool canPlaceOrder { get; set; } = false;

        public Manager()
        {
            tradeDecHead.SetNext(tradeDecSwing).SetNext(tradeDecTail);
        }


        public async Task<bool> AddSecurityToMarket(Project project)
        {
            string market = project.market;
            if (!markets.ContainsKey(market))
                return false;

            string candleCode = await markets[market].AddSecurity(project);
            if (candleCode.Equals(""))
                return false;

            project.candleCode = candleCode;
            return true;
        }


        public bool RemoveProject(string name)
        {
            if (projects.TryGetValue(name, out var project))
            {
                projects.Remove(name);
                outputs.Remove(name);
                return true;
            }
            return false;
        }


        public bool AddProject(string name, string market, string p1, string p2, int period)
        {
            if (projects.ContainsKey(name))
                return false;

            var p = new Portfolio(p1, p2, 0, 100, 100, 0.001m, 0.001m); // these are default values, not ones that reflect the account (change this)
            string output = $"{filePath}{name}_{market}_{p1}_{p2}.csv";
            
            // Project with given name already exists with the market, and coin-pair
            if (outputs.TryGetValue(output, out string? _name))
                return false;

            var project = new Project(tradeDecHead, p, market, name, period);
            outputs.Add(name, output);
            projects.Add(name, project);
            return true;
        }


        // If no data exists within the project, fill data with Market Coin-Pair data
        public bool TryProjectFill(Project project, List<Candle> candles)
        {
            if (project.data.Count != 0)
                return false;
            
            for (int i = 0; i < candles.Count; ++i)
                project.ProcessCandle(candles[i], false);

            ProcessFile.ProcessAll(outputs[project.name], project);
            return true;
        }


        public void UpdateLatestCandle(string market, string candleCode, List<string> latestCandle)
        {
            foreach (var project in projects.Values)
            {
                if (!project.market.Equals(market))
                    continue;

                if (!project.candleCode.Equals(candleCode))
                    continue;

                if (!project.initialUpdate) project.initialUpdate = true;

                project.latestCandle = latestCandle;
            }
        }

        public string currPrint { get; set; } = "";

        public void UpdateLatestMark(string market, string candleCode, decimal askPrice, decimal bidPrice)
        {
            foreach (var project in projects.Values)
            {
                if (!project.market.Equals(market))
                    continue;

                if (!project.futureCandleCode.Equals(candleCode))
                    continue;
                
                // DO EMERGENCY SELL STUFF:
                if(!markets[market].orders.TryGetValue(project.clientId, out var order))
                    continue;

                var datum = markets[market].symbolData[project.futureCandleCode];
                decimal multi = (decimal)datum.multiplier;

                decimal entry = order.entry;
                decimal percentChange = (askPrice - entry) / entry;
                decimal value = entry * multi * order.size;
                string tempSide = "sell";
                string position = order.side;

                decimal takeProfit = project.takeProfit;
                decimal dynamicPercent = project.dynamicPercent;

                if (order.side.Equals("sell"))
                {
                    percentChange = -percentChange;
                    tempSide = "buy";
                }
                

                string newTxt = $"Value: [{value}]  |  Entry: [{entry}]  |  AskPrice: [{askPrice}]  |  Change: {(percentChange*order.leverage*100).ToString("0.000")}%";
                if (!currPrint.Equals(newTxt))
                {
                    currPrint = newTxt;
                    Console.WriteLine(currPrint);
                }
                /*
                string atrSmoothStatus = project.data.Last().smoothDecision;
                if(project.position == "" && atrSmoothStatus.Equals("buy"))
                {
                    Candle latest = project.data[project.data.Count - 1];
                    latest.finalDecision = "-"; // This implies that the current position was sold (neutral)
                    project.position = "";
                    markets[market].PlaceOrder(project, atrSmoothStatus, false);        
                }
                if(project.position == "" && atrSmoothStatus.Equals("sell"))
                {
                    Candle latest = project.data[project.data.Count - 1];
                    latest.finalDecision = "-"; // This implies that the current position was sold (neutral)
                    project.position = "";
                    markets[market].PlaceOrder(project, atrSmoothStatus, false);        
                }

                // First time that percentage change exceeds take profit (only happens once)
                if (percentChange > takeProfit && dynamicPercent == 0)
                {
                    project.dynamicPercent = percentChange;
                    project.stopLoss = takeProfit / 4; // CHANGE HERE

                // After take profit is exceeded once, increase stop loss everytime that current candle price > previous candle price
                } else if (dynamicPercent != 0 && percentChange > dynamicPercent)
                {
                    project.stopLoss += ((percentChange - dynamicPercent) / 2); // CHANGE HERE

                // Stoploss converges toward current candle price at logarithmic rate
                } else if (project.currPercentChange != 0 && percentChange < project.currPercentChange && !(percentChange < -project.stopLoss))
                {
                    project.stopLoss += Math.Abs(askPrice - entry) / 6; // CHANGE HERE
                }
                project.currPercentChange = percentChange;


                if (percentChange < -project.stopLoss)
                {
                    Candle latest = project.data[project.data.Count - 1];
                    latest.finalDecision = "-"; // This implies that the current position was sold (neutral)
                    project.position = "";
                    project.dynamicPercent = 0;
                    markets[market].PlaceOrder(project, tempSide, false);
                }*/


            }
        }

        public async Task AddNewCandle(string market, string candleCode, string uriCode, List<string> latestCandle)
        {
            foreach (var project in projects.Values)
            {
                if (!project.market.Equals(market))
                    continue;

                if (!project.candleCode.Equals(candleCode))
                    continue;
                
                int latestTime = (int)project.data[project.data.Count - 1].unix;
                int diff = int.Parse(latestCandle[0]) - latestTime;
                int period = project.period;

                // Missing data due to socket issue or poor network connection
                if (diff > (60 * period * 2) || !project.initialUpdate)
                {
                    Console.WriteLine("Data fill required.");
                    if (!project.initialUpdate) project.initialUpdate = true;

                    string tempUri = markets[market].GetUri(period, diff, uriCode);
                    var candles = await markets[market].GetDataFill(tempUri, diff/(60 * period));
                    for (int i = 0; i < candles.Count; ++i)
                    {
                        Console.WriteLine($"Filling Data: {candles[i].unix} --------");
                        bool placeOrder = i == candles.Count - 1 ? true : false;
                        AddNewCandleHelper(project, candles[i], placeOrder);
                    }
                }
                else
                {
                    Candle? newCandle = markets[market].CreateCandle(project.latestCandle);
                    if (newCandle == null)
                    {  
                        Console.WriteLine("NEW CANDLE BECAME NULL"); // not sure why this happens or when...
                        return;
                    }

                    Console.WriteLine($"No data fill needed: {newCandle.unix}");
                    AddNewCandleHelper(project, newCandle, true);
                }
            }
        }

        public void AddNewCandleHelper(Project project, Candle candle, bool placeOrder)
        {
            if (project.ProcessCandle(candle, true))
            {
                if (canPlaceOrder)
                {

                    if (!placeOrder && (candle.finalDecision.Equals("sell") || candle.finalDecision.Equals("buy"))) // happens during data fill
                    {
                        string decision = candle.finalDecision;
                        candle.finalDecision = "-"; // Because this is part of datafill, we don't want to buy or sell here
                        markets[project.market].PlaceOrder(project, decision, false);
                    }
                    else if (placeOrder) // happens for every new candle and last candle in data fill
                        markets[project.market].PlaceOrder(project, candle.finalDecision, true);
                }
                ProcessFile.ProcessNext(outputs[project.name], project);
            }
        }
    }
}