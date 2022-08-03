namespace TradingBot
{
    public class Candle
    {
        /* General Candle Data */
        public long unix { get; set; }
        public decimal open { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal close { get; set; }


        /* General Data */
        public decimal pivotHi { get ; set; }
        public decimal pivotLo { get; set; }


        /* Higher Lower Data */
        public decimal? hl { get; set; } // Trend direction
        public decimal? zz { get; set; } // Zig zag curve
        public string hilo { get; set; } = "-";
        public string hiloDecision { get; set; } = "-";


        /* Macd Signal Data */
        public decimal? signal { get; set; }
        public decimal? macd { get; set; }
        public decimal? hist { get; set; }
        public decimal? fast { get; set; }
        public decimal? slow { get; set; }
        public string macDecision { get; set;} = "-";


        /* Relative Strength Index */
        public decimal? rsi {get; set; }

        
        public Candle(int ux, decimal op, decimal hi, decimal lo, decimal cl)
        {
            unix = ux;
            open = op;
            high = hi;
            low = lo;
            close = cl;
        }
    }
}