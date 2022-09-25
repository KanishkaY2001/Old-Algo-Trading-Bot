using Newtonsoft.Json;
using TradingBot;

namespace KuCoinFiles
{
    public class KuCoin : IMarket
    {
        // General Information //
        public Dictionary<string,List<Candle>> securities { get; set; }
        public int storageAmount { get; set; } = 200;
        public string code { get; set; } = "-";
        public string market { get; set; } = "KuCoin";


        // Websocket information //
        public string wss { get; set; } = "";
        public static string id { get; set; } = "";
        public WSocket socket { get; set; }
        public List<string> prevCandle { get; set; } = new List<string>();
        public string[] reqParams { get; set; } = new string[]
        {
            "{ \"id\": ", // followed by id
            ",\"type\": \"subscribe\", \"topic\": \"/market/candles:", // followed by string.Join(",", subs)
            "\", \"privateChannel\": false, \"response\": true}"
        };


        // Rest API Information //
        public string get { get; set; } = "https://api.kucoin.com/api";
        public string post { get; set; } = "https://api.kucoin.com/api/v1/bullet-public";
        public string[] uriParams { get; set; } = new string[] 
        {
            "/v1/market/candles?type=1min&symbol=",
            "&startAt=",
            "&endAt="
        };
        
        public KuCoin()
        {
            securities = new Dictionary<string,List<Candle>>();
            SetupWebsocket();
            socket = new WSocket(this);
        }

        private async void SetupWebsocket()
        {
            var result = await RestApi.PostJson(post, 0, "", "");
            var decerialized = JsonConvert.DeserializeObject<Token>(result);
            if (decerialized == null)
            {
                Console.WriteLine("Failed to initialize KuCoin!");
                System.Environment.Exit(1);
            } 

            var data = decerialized.data;
            var servers = data.instanceServers;
            wss = $"{servers[0].endpoint}?token={data.token}";
        }


        private string GetUri(int duration, string security)
        {
            long to = Helper.GetUnix();
            long from = to - duration;
            return $"{get}{uriParams[0]}{security}{uriParams[1]}{from}{uriParams[2]}{to}";
        }


        public void UpdateManager(Candle candle, string security)
        {
            Manager.Global.UpdateProjects(candle, market, security);
        }


        private void AddCandle(Candle candle, string security)
        {
            var securityData = securities[security];
            // Adds the next candle
            if (securityData.Count == storageAmount)
            {
                securityData.RemoveAt(0);
            }
            securityData.Add(candle);

            // Update the manager with new info
            UpdateManager(candle, security);
        }


        private Candle CreateCandle(List<string> rawData)
        {
            return new Candle(new string[]
            {
                rawData[0],
                rawData[1],
                rawData[3],
                rawData[4],
                rawData[2],
            });
        }


        public void SocketMessage(string msg)
        {   
            var decerialized = JsonConvert.DeserializeObject<WSKline>(msg);
            if (decerialized == null)
                return;
            
            if (decerialized.type.Equals("welcome"))
            {
                id = decerialized.id; // currently, I'm sending message before this even takes effect
            }

            switch (decerialized.subject)
            {
                case "trade.candles.update":
                    prevCandle = decerialized.data.candles; // Only works for a single coin-pair, (1 market = 1 coin-pair) , need ot make this a list to add more
                    break;
                case "trade.candles.add":
                    // Update new candle has been added:
                    Candle newCandle = CreateCandle(prevCandle);
                    AddCandle(newCandle, decerialized.data.symbol);
                    break;
            }
        }
        

        public string createRequest(List<string> subs)
        {
            string sub = string.Join(",", subs);
            return $"{reqParams[0]}0{reqParams[1]}{sub}{reqParams[2]}";
        }


        public async Task<bool> AddSecurity(string p1, string p2, string period)
        {
            string secCode = $"{p1}{code}{p2}";

            // Ensure that the security doesn't already exist
            if (securities.ContainsKey(secCode)) return false;
            
            // Setup the list of candles and the amount to collect
            List<Candle> candles = new List<Candle>();

            // Create url and get JSON response with GET
            string json = await RestApi.GetJson(GetUri((storageAmount * 60), secCode));

            // Create an object from the JSON result and ensure validity
            var decerialized = JsonConvert.DeserializeObject<Kline>(json);
            if (decerialized == null || decerialized.data.Count == 0) 
                return false;

            // Loop through all KLines and create candles out of them
            for (int i = decerialized.data.Count - 1; i >= 0; --i)
            {
                candles.Add(CreateCandle(decerialized.data[i]));
            }

            // Create the full list of candles (previous data)
            securities.Add(secCode, candles);
            
            // Update project with data and add data to output file
            UpdateManager(null!, secCode);

            // Start websocket stream
            socket.subs.Add($"{secCode}_{period}");
            socket.StartStream(wss, createRequest(socket.subs));

            return true;
        }
    }
}