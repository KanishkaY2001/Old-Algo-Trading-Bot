namespace TradingBot
{
    public class Portfolio
    {
        /* Tokens Being Used */
        public string pairA { get; set; }
        public decimal valueA { get; set; } 
        public string pairB { get; set; }
        public decimal valueB { get; set; } 

        /* Allowance | Buy Limit */
        public decimal allowance { get; set; }

        /* Transaction Fees */
        public decimal maker { get; set; }
        public decimal taker { get; set; }

        /* Profitability */
        public decimal allProfit { get; set; }

        /* Order Book-keeping */
        public decimal buyOrder { get; set; }
        public decimal stopLoss { get; set; }

        public Portfolio(
            string nA, 
            string nB, 
            decimal vA, 
            decimal vB, 
            decimal all, 
            decimal m, 
            decimal t
        ) {   
            pairA = nA;
            pairB = nB;
            valueA = vA;
            valueB = vB;
            allowance = all;
            maker = m;
            taker = t;
        }
    }
}