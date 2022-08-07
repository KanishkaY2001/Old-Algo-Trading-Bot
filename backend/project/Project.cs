namespace TradingBot
{
    public class Project
    {
        public List<Candle> data { get; set; } = new List<Candle>();
        public int maxDataLen { get; set; } = 50; // Everything else is removed
        public Portfolio portfolio { get; set; }
        public Dictionary<long,string> snapshots = new Dictionary<long, string>();
        public TaskHandler tradeDecision { get; set; }
        public string dataStreamId { get; set; } = "";

        /* Indicator Optimisation Storage */
        public GeneralOpt genOpt { get; set; } = new GeneralOpt();
        public HighLowOpt hiloOpt { get; set; } = new HighLowOpt(9);
        public MacdSigOpt macdSOpt { get; set; } = new MacdSigOpt(12, 26, 9);
        public RsiOpt rsiOpt { get; set; } = new RsiOpt(14, 14);
        public StochRsiOpt stochRsiOpt { get; set; } = new StochRsiOpt(14, 3);

        public Project(TaskHandler decision, Portfolio _portfolio)
        {
            portfolio = _portfolio;
            tradeDecision = decision;
            tradeDecision.HandleTask(this);
        }
        
        /* Buy & Sell Methods */
        public void NormalBuy()
        {
            Candle candle = data.Last();
            portfolio.stopLoss = data[data.Count - 3].close;
            portfolio.buyOrder = candle.close;

            /* allowance allows constant buy amount at set price */
            decimal buy = Math.Min(portfolio.allowance, portfolio.valueB);
            buy = portfolio.allowance != 0? buy : portfolio.valueB;
            
            /* Buy pairA using pairB */
            portfolio.valueA = buy / portfolio.buyOrder;
            portfolio.valueB -= buy;
            candle.finalDecision = "buy";

            /* Add a snapshot of the portfolio */
            snapshots.Add(candle.unix,$"{portfolio.valueA:0.###},{portfolio.valueB:0.###}");
        }

        public void NormalSell()
        {
            Candle candle = data.Last();

            /* Sell pairA and get pairB */
            portfolio.valueB += portfolio.valueA * candle.close;
            portfolio.valueA = 0;
            candle.finalDecision = "sell";

            /* Add a snapshot of the portfolio */
            decimal profit = (candle.close / portfolio.buyOrder - 1) * 100;
            portfolio.allProfit += profit;
            snapshots.Add(candle.unix,
            $"{portfolio.valueA:0.###},{portfolio.valueB:0.###},{profit:0.###}%,{portfolio.allProfit:0.###}%");
        }

        public void EmergencySell()
        {
            if (data.Last().close < portfolio.stopLoss)
                NormalSell();
        }


        /* Candle Methods */
        public void ProcessCandle(Candle candle)
        {
            if (data.Count == 0)
                snapshots.Add(candle.unix,$"{portfolio.valueA:0.###},{portfolio.valueB:0.###}");

            data.Add(candle);
            MakeTradeDecision();
        }

        public void MakeTradeDecision()
        {
            /* Apply HiLo Indicator */
            HighLow.ApplyIndicator(this);

            /* Apply MacdSignal Indicator */
            MacdSig.ApplyIndicator(this);

            /* Apply Rel Str Idx Indicator */
            RelStrIdx.ApplyIndicator(this);

            /* Apply Stoch Rsi Indicator */
            StochRsi.ApplyIndicator(this);

            /* Make Trade Decision */
            tradeDecision.HandleTask(this);
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