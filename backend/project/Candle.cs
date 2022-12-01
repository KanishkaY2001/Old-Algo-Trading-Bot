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
        public decimal volume { get; set; }

        /* General Data */
        public decimal? pivH { get ; set; }
        public decimal? pivL { get; set; }
        public string finalDecision { get; set; } = "-";

        /* Higher Lower Data */
        public decimal? hl { get; set; } // Trend direction
        public decimal? zz { get; set; } // Zig zag curve
        public string hilo { get; set; } = "-";
        public string cross { get; set; } = "-";
        public string hiloDecision { get; set; } = "-";

        /* Macd Signal Data */
        public decimal? signal { get; set; }
        public decimal? macd { get; set; }
        public decimal? hist { get; set; }
        public decimal? fast { get; set; }
        public decimal? slow { get; set; }
        public string macDecision { get; set; } = "-";

        /* Relative Strength Index */
        public decimal? rsi {get; set; }
        public decimal? rsiMA { get; set; }
        public string rsiDecision { get; set; } = "-";

        /* Chandalier Exit Data */
        public string chandDecision { get; set; } = "-";

        /* ATR Smoothed */
        public string smoothDecision { get; set; } = "-";

        /* Stochastic RSI Data */

        public decimal? K { get; set; }
        public decimal? D { get; set; }

        public Candle(long ux, decimal op, decimal hi, decimal lo, decimal cl, decimal v)
        {
            unix = ux;
            open = op;
            high = hi;
            low = lo;
            close = cl;
            volume = v;
        }

        public Candle(string[] data)
        {
            try
            {
                unix = long.Parse(data[0]);
                open = decimal.Parse(data[1]);
                high = decimal.Parse(data[2]);
                low = decimal.Parse(data[3]);
                close = decimal.Parse(data[4]);
                volume = decimal.Parse(data[5]);
            } catch (Exception e)
            {
                Console.WriteLine($"-- Unable to parse: {string.Join(',',data)} --\n{e}");
            }
        }
    }
}