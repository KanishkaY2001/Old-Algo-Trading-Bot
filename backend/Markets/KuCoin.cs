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


        private string GetUri(int duration, string security)
        {
            long to = Helper.GetUnix();
            long from = to - duration;
            var url = $"{uri}{uriParams[0]}{security}{uriParams[1]}{from}{uriParams[2]}{to}";
            return url;
        }


        public void UpdateManager(Candle candle, string market)
        {
            Manager.Global.UpdateProjects(candle, market);
        }


        private Candle CreateCandle(Kline decerialized, int row)
        {
            string[] data = new string[]
            {
                decerialized.data[row][0],
                decerialized.data[row][1],
                decerialized.data[row][3],
                decerialized.data[row][4],
                decerialized.data[row][2],
            };
            return new Candle(data);
        }


        public async Task<bool> AddSecurity(string security)
        {
            // Ensure that the security doesn't already exist
            if (securities.ContainsKey(security)) return false;
            
            // Setup the list of candles and the amount to collect
            List<Candle> candles = new List<Candle>();

            // Create url and get JSON response with GET
            string json = await RestApi.GetJson(GetUri((storageAmount * 60), security));

            // Create an object from the JSON result and ensure validity
            var decerialized = JsonConvert.DeserializeObject<Kline>(json);
            if (decerialized == null || decerialized.data.Count == 0) 
                return false;

            // Loop through all KLines and create candles out of them
            for (int i = decerialized.data.Count - 1; i > 0; --i)
            {
                candles.Add(CreateCandle(decerialized, i));
            }

            // Create the full list of candles (previous data)
            securities.Add(security, candles);

            // Start loop to collect kline data every minute
            System.Timers.Timer timer = new System.Timers.Timer(5000);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(async (sender, args) => 
            {
                json = await RestApi.GetJson(GetUri(60, security));
                decerialized = JsonConvert.DeserializeObject<Kline>(json);
                if (decerialized == null || decerialized.data.Count == 0)
                {
                    Console.WriteLine("Decerialization Failure");
                    return;
                }

                var candle = CreateCandle(decerialized, 0);

                // Add candle to list:
                var securityData = securities[security];
                if (securityData.Count == 200)
                {
                    securityData.RemoveAt(securityData.Count - 1);
                    securityData.Add(candle);
                }

                UpdateManager(candle, $"KuCoin-{security}");
                Console.WriteLine(json);
            });

            timer.Start();

            return true;
        }
    }
}