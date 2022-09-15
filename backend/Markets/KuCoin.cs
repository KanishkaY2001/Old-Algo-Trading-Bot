using Newtonsoft.Json;

namespace TradingBot
{
    public class KuCoin : IMarket
    {
        public int storageAmount { get; set; } = 200;
        public string uri { get; set; } = "https://api.kucoin.com/api";
        public string[] uriParams { get; set; } = new string[] {
            "/v1/market/candles?type=1min&symbol=",
            "&startAt=",
            "&endAt="
        };
        public Dictionary<string,List<Candle>> securities { get; set; }
        public KuCoin()
        {
            securities = new Dictionary<string,List<Candle>>();
        }

        class Kline
        {
            public string code { get; set; } = "";
            public List<List<string>> data { get; set; }

            public Kline()
            {
                code = "";
                data = new List<List<string>>();
            }
        }

        public Candle GetKline()
        {
            return new Candle(1,1,1,1,1);
        }

        public async Task<bool> AddSecurity(string security)
        {
            // Ensure that the security doesn't already exist
            if (securities.ContainsKey(security)) return false;
            
            // Setup the list of candles and the amount to collect
            List<Candle> candles = new List<Candle>();
            long to = Helper.GetUnix();
            long from = to - (storageAmount * 60);

            // Create url and get JSON response with GET
            var url = $"{uri}{uriParams[0]}{security}{uriParams[1]}{from}{uriParams[2]}{to}";
            string json = await RestApi.GetJson(url);

            // Create an object from the JSON result and ensure validity
            var decerialized = JsonConvert.DeserializeObject<Kline>(json);
            if (decerialized == null || decerialized.data.Count == 0) return false;

            // Loop through all KLines and create candles out of them
            for (int i = decerialized.data.Count - 1; i > 0; --i)
            {
                string[] data = new string[]
                {
                    decerialized.data[i][0],
                    decerialized.data[i][1],
                    decerialized.data[i][3],
                    decerialized.data[i][4],
                    decerialized.data[i][2],
                };
                candles.Add(new Candle(data));
            }

            // Create the full list of candles (previous data)
            securities.Add(security, candles);
            return true;
        }
    }
}