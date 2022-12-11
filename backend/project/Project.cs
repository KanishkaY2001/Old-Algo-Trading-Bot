namespace TradingBot
{
    public class Project
    {
        public List<Candle> data { get; set; } = new List<Candle>();
        public List<string> latestCandle { get; set; } = new List<string>();
        public bool backTesting { get; set; } = false;
        public string clientId { get; set; } = "";
        public string position { get; set; } = "-";
        public decimal currPercentChange { get; set; } = 0;
        public decimal dynamicPercent { get; set; } = 0;
        public decimal takeProfit { get; set; } = 2m; // // CHANGE HERE
        public decimal stopLoss { get; set; } = 10m; // // CHANGE HERE
        public int latestTime { get; set; }
        public int maxDataLen { get; set; } = 50; // Everything else is removed
        public Portfolio portfolio { get; set; }
        public Dictionary<long,string> snapshots = new Dictionary<long, string>();
        public TaskHandler tradeDecision { get; set; }
        public string market { get; set; }
        public string candleCode { get; set; } = "";
        public string futureCandleCode { get; set; } = "";
        public string name { get; set; }
        public int period { get; set; }
        public bool initialUpdate { get; set; }

        /* Indicator Optimisation Storage */
        public GeneralOpt genOpt { get; set; } = new GeneralOpt();
        public HighLowOpt hiloOpt { get; set; } = new HighLowOpt(9);
        public MacdSigOpt macdSOpt { get; set; } = new MacdSigOpt(12, 26, 9);
        public RsiOpt rsiOpt { get; set; } = new RsiOpt(14, 14);
        public StochRsiOpt stochRsiOpt { get; set; } = new StochRsiOpt(14, 3);
        public ChandelierOpt chandalierOpt { get; set; } = new ChandelierOpt(22, 3m); // 22 3m
        public atrSmoothOpt atrSmoothOpt { get; set; } = new atrSmoothOpt(21, 6.3m);
        public SwingArmOpt swingArmOpt {get ;set; } = new SwingArmOpt(28, 7, true);

        public Project(TaskHandler _d, Portfolio _p, string _m, string _n, int _pr, bool _bt)
        {
            name = _n;
            market = _m; // market
            portfolio = _p;
            period = _pr;
            tradeDecision = _d;
            backTesting = _bt;
            tradeDecision.HandleTask(this);
        }


        public void AddSnap(long unix, string input)
        {
            if (snapshots.TryGetValue(unix, out string? d))
            {
                snapshots[unix] = input;
                return;
            }   
            snapshots.Add(unix, input);
        }

        
        /* Buy & Sell Methods */
        public void NormalBuy()
        {
            Candle candle = data.Last();
            //portfolio.stopLoss = data[data.Count - 3].close;
            
            /*
            if (Dummy.positionStatus.Equals("Short"))
            {
                portfolio.stopLoss = candle.close + candle.close * 0.1m;
            }
            else if (Dummy.positionStatus.Equals("Long"))
            {
                portfolio.stopLoss = candle.close - candle.close * 0.1m;
            }
            */
            
            /* allowance allows constant buy amount at set price */
            decimal buy = Math.Min(portfolio.allowance, portfolio.valueB);
            buy = portfolio.allowance != 0? buy : portfolio.valueB;
            
            /* Buy pairA using pairB */
            portfolio.valueB -= buy;
            candle.finalDecision = "buy";
            

            /* Add a snapshot of the portfolio */
            if (!position.Equals("-")) AddPnL();
            position = "buy";
            portfolio.buyOrder = candle.close;
        }

        public void NormalSell()
        {
            Candle candle = data.Last();
            
            /* Sell pairA and get pairB */
            portfolio.valueB += portfolio.valueA * candle.close;
            portfolio.valueA = 0;
            candle.finalDecision = "sell";
            

            /* Add a snapshot of the portfolio */
            if (!position.Equals("-")) AddPnL();
            position = "sell";
            portfolio.buyOrder = candle.close;

        }

        public bool Emergency(decimal askPrice, decimal entry)
        {
            // Emergency Sell Logic
            var candle = data.Last();
            if (!candle.finalDecision.Equals("-"))
            {
                decimal percentChange = (askPrice - entry) / entry;
                if (position.Equals("sell")) percentChange *= -1;

                Console.WriteLine($"{Helper.UnixToDate(data.Last().unix)}  ||  {portfolio.buyOrder}  ||  Change: {percentChange}");
                
                if (percentChange > takeProfit || percentChange < -stopLoss)
                {
                    position = "-";
                    candle.finalDecision = "-";
                    AddPnL();
                    return true;
                }
            }
            return false;
        }

        public void AddPnL()
        {
            var candle = data.Last();
            decimal profit = (candle.close / portfolio.buyOrder - 1) * 100;
            portfolio.allProfit += profit;
            AddSnap(candle.unix, $"{portfolio.valueA:0.###},{portfolio.valueB:0.###},{profit:0.###}%,{portfolio.allProfit:0.###}%");
        }


        /* Candle Methods */
        public bool ProcessCandle(Candle candle, bool canTrade)
        {
            // Data fill is consistent (time spacing)
            if (data.Count > 0)
            {
                if (candle.unix - data.Last().unix != period * 60)
                    return false;
            }

            if (data.Count == 0)
                AddSnap(candle.unix, $"{portfolio.valueA:0.###},{portfolio.valueB:0.###}");
                
            data.Add(candle);
            MakeTradeDecision(canTrade);

            // Backtest printing:
            string macd = candle.macd != null ? ((decimal)candle.macd).ToString("0.00") : "-";
            string signal = candle.signal != null ? ((decimal)candle.signal).ToString("0.00") : "-";
            string hist = candle.hist != null ? ((decimal)candle.hist).ToString("0.00") : "-";
            string k = candle.K != null ? ((decimal)candle.K).ToString("0.00") : "-";
            string d = candle.D != null ? ((decimal)candle.D).ToString("0.00") : "-";

            string xyz = "";
            for (int i = 0; i < hiloOpt.pattern.Count(); ++i)
            {
                if (!hiloOpt.pattern[i].Equals("-"))
                {
                    if (xyz.Equals(""))
                        xyz = $"{hiloOpt.pattern[i]}";
                    else
                        xyz = $"{xyz},{hiloOpt.pattern[i]}";
                }
            }
            //Console.WriteLine($"[{candle.chandDecision}], [{xyz}], [{macd}, {signal}, {hist}], [{k}, {d}]");

            return true;
        }

        public void MakeTradeDecision(bool canTrade)
        {
            /* Apply HiLo Indicator */
            HighLow.ApplyIndicator(this);

            /* Apply MacdSignal Indicator */
            MacdSig.ApplyIndicator(this);

            /* Apply Rel Str Idx Indicator */
            RelStrIdx.ApplyIndicator(this);

            /* Apply Stoch Rsi Indicator */
            StochRsi.ApplyIndicator(this);

            /* Apply Chandelier Indicator */
            Chandelier.ApplyIndicator(this);

            /* Apply ATR Smoothed Indicator */
            atrSmooth.ApplyIndicator(this);

            /* Apply SwingArm Indicator */
            SwingArm.ApplyIndicator(this);
            
            /* Make Trade Decision */
            if (!canTrade) return;
                
            tradeDecision.HandleTask(this);

            /* Backtesting emergency exit */
            if (backTesting) Emergency(data.Last().close, portfolio.buyOrder);




            /*
            decimal percentChange = (askPrice - entry) / entry;
            if (percentChange > takeProfit || percentChange < -stopLoss)     // if( 0.0121 > 0.012)
            {
                Candle latest = project.data[project.data.Count - 1];
                latest.finalDecision = "-"; // This implies that the current position was sold (neutral)
                project.position = "";
                project.dynamicPercent = 0;
                markets[market].PlaceOrder(project, tempSide, false);
            }
            */



            /*
            Candle candle = data[data.Count() - 1];
            var chand = candle.chandDecision;
            decimal macd = 0;
            decimal sig = 0;
            var prevCross = "-";
            for (int i = data.Count() - 1; i >= 0; --i)
            {
                if (!data[i].cross.Equals("-") && prevCross.Equals("-"))
                {
                    prevCross = data[i].cross;
                    macd = data[i].macd != null ? (decimal)data[i].macd! : 0;
                    sig = data[i].signal != null ? (decimal)data[i].signal! : 0;

                    //Console.WriteLine($"{position} ... {macd} ... {sig} ... {prevCross} ... {chand}");
                    string atrSmoothStatus = data.Last().smoothDecision;
                    */

                    /*
                    if ((position.Equals("")) && ((macd < 0 || sig < 0) && (prevCross.Equals("green")) && (chand.Equals("buy"))))
                    {
                        Console.WriteLine($"BUYING LONG POSITION AT: {candle.unix}-----------------------------------------------");

                        candle.finalDecision = "buy";
                        position = "long";
                    }
                    */                    
                    /*
                    else if ((position.Equals("long")) && ((prevCross.Equals("red") && macd < 0) || (chand.Equals("sell"))))
                    {
                        
                        Console.WriteLine($"SELLING LONG POSITION AT: {candle.unix}-----------------------------------------------");

                        candle.finalDecision = "sell";
                        position = "";
                    }
                    */
                    /*else if ((position.Equals("")) && ((macd > 0 || sig > 0) && (prevCross.Equals("red")) && chand.Equals("sell")))
                    {
                        Console.WriteLine($"BUYING SHORT POSITION AT: {candle.unix}-----------------------------------------------");

                        candle.finalDecision = "sell";
                        position = "short";
                    }*/
                    /*
                    else if ((position.Equals("short")) && ((prevCross.Equals("green") && macd > 0) || chand.Equals("buy")))
                    {
                        Console.WriteLine($"SELLING SHORT POSITION AT: {candle.unix}-----------------------------------------------");
                        
                        candle.finalDecision = "buy";
                        position = "";
                    }
                    */
                //}
            //}       
        }


        /* Pinescript Methods */
        public decimal? PivotHigh()
        {
            return PineSim.GetPivotHighLow(this, true);
        }

        public decimal? PivotLow()
        {
            return PineSim.GetPivotHighLow(this, false);
        }

        public decimal? ValueWhen(string cond, string src, int ocr)
        {
            return (decimal?)PineSim.GetValueWhen(data, cond, src, ocr);
        }

        public decimal? Past(string src, int back)
        {
            return PineSim.GetPastValue(data, src, back);
        }

        public decimal? Sma(string src, int len)
        {
            return PineSim.GetSma(data, src, len);
        }

        public decimal? Ema(string src, string prop, int len)
        {
            return PineSim.GetEma(data, src, prop, len);
        }
    }
}