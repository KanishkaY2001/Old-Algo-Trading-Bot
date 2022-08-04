namespace TradingBot
{
    public class Project
    {
        public List<Candle> data { get; set; } = new List<Candle>();
        public Portfolio portfolio { get; set; }
        public TaskHandler tradeDecision { get; set; }
        public string dataStreamId { get; set; } = "";

        /* Indicator Profiles */

        /* General */
        public int leftBars { get; set; } = 2;
        public int rightBars { get; set; } = 2;
        
        /* HighLow */
        public int maxPattern { get; set; } = 9;
        public List<string> HiLoPattern { get; set; } = new List<string>();

        /* MacdSig */
        public decimal? macdSum { get; set; } = 0;
        public decimal? fastSum { get; set; } = 0;
        public decimal? slowSum { get; set; } = 0;
        public List<decimal> smaMacd { get; set; } = new List<decimal>();
        public List<decimal> smaFast { get; set; } = new List<decimal>();
        public List<decimal> smaSlow { get; set; } = new List<decimal>();

        /* RelStrIdx */
        public decimal up { get; set; }
        public decimal down { get; set; }
        public decimal rsiSum { get; set; }
        public Queue<decimal?> rsiQ = new Queue<decimal?>();

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

            /* Maker fees (Loss) */
            portfolio.loss += buy * portfolio.maker;
        }

        public void NormalSell()
        {
            Candle candle = data.Last();
            decimal sellOrder = candle.close;
            decimal sell = portfolio.valueA * sellOrder;
            
            /* Determine if the trade was good or bad */
            if (sellOrder > portfolio.buyOrder)
                ++portfolio.goodTrades;
            else
                ++portfolio.badTrades;

            /* Sell pairA and get pairB */
            portfolio.valueB += sell;
            portfolio.valueA = 0;
            candle.finalDecision = "sell";

            /* Taker fees (Loss) */
            portfolio.loss += sell * portfolio.taker;

            /* Gross Revenue (P/L) */
            decimal gross = (sellOrder - portfolio.buyOrder) / portfolio.buyOrder;
            portfolio.revenueRate += gross;
        }

        public void EmergencySell()
        {
            if (data.Last().close < portfolio.stopLoss)
                NormalSell();
        }


        /* Candle Methods */
        public void ProcessCandle(Candle candle)
        {
            data.Add(candle);
            MakeTradeDecision();
        }

        public void MakeTradeDecision()
        {
            /* Apply HiLo Indicator */
            HighLow.ApplyIndicator(this);

            /* Apply MacdSignal Indicator */
            //MacdSig.ApplyIndicator(this);

            /* Apply Rel Str Idx Indicator */
            //RelStrIdx.ApplyIndicator(this);

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