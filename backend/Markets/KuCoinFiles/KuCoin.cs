using Newtonsoft.Json;
using TradingBot;
using System.Security.Cryptography;
using System.Net;
using System.Text;

namespace KuCoinFiles
{
    public class KuCoin : IMarket
    {
        // General Information //
        public List<string> securities { get; set; } = new List<string>();
        public Dictionary<string,string> futureSecurities { get; set; } = new Dictionary<string,string>();
        public Dictionary<string, Order> orders { get; set; } = new Dictionary<string, Order>();
        public int storageAmount { get; set; } = 100;
        public string market { get; set; } = "KuCoin";
        public Dictionary<string, string> periods { get; set; } = new Dictionary<string, string>()
        {
            { "1" , "1min" },
            { "3" , "3min" },
            { "5" , "5min" },
            { "15" , "15min" },
            { "30" , "30min" },
            { "60" , "1hour" },
        };

        // Secret Information //
        public static string api_key = "6359023df3f40e00018ae3ce";
        public static string api_secret = "5c6d3def-37d0-4b01-8a6d-b828b2509d44";
        public static string api_passphrase = "CleanSlate2001";
        public static string domain = "https://api-futures.kucoin.com";



        // Websocket information //
        public string wss { get; set; } = "";
        public static string id { get; set; } = "";
        public WSocket ohlcSocket { get; set; }
        public Task ohlcSocketTask { get; set; }
        public WSocket markSocket { get; set; }
        public Task markSocketTask { get; set; }
        public string[] reqParams { get; set; } = new string[]
        {
            "{ \"id\": ", // followed by id
            ",\"type\": \"subscribe\", \"topic\": \"/market/candles:", // followed by string.Join(",", subs)
            "\", \"privateChannel\": false, \"response\": true}"
        };
        public string[] markParams { get; set; } = new string[]
        {
            "{ \"id\": ",
            ",\"type\": \"subscribe\", \"topic\": \"/contractMarket/tickerV2:",
            "\", \"response\": true}"
        };

        // have this for every token.
        // This is important because sometimes, after adding a new token, there may not be a trade.candles.update, which means prevCandle is
        // not updated to what it should be. THis happens for coins which are not popular. If the initialUpdate doesn't happen, then we need
        // a datafill compulsory. After the datafill, the initialUpdate has happened. Think of this like a one-off, that's required for each token.
        public bool initialUpdate = false;


        // Rest API Information //
        public string get { get; set; } = "https://api.kucoin.com/api";
        public string post { get; set; } = "https://api.kucoin.com/api/v1/bullet-public";

        /*
        public string[] uriParams { get; set; } = new string[] 
        {
            "/v1/market/candles?type=1min&symbol=",
            "&startAt=",
            "&endAt="
        };*/
        public string[] uriParams { get; set; } = new string[] 
        {
            "/v1/market/candles?type=", // 1, 3, 5 (minutes)
            "min&symbol=",
            "&startAt=",
            "&endAt="
        };
        
        public KuCoin()
        {
            SetupWebsocket();
            ohlcSocket = new WSocket(this);
            markSocket = new WSocket(this);
            ohlcSocketTask = new Task( () => ohlcSocket.StartStream(wss, createOhlcRequest()));
            markSocketTask = new Task( () => markSocket.StartStream(wss, createMarkRequest()));
        }

        public void RestartSocketTask()
        {
            // Dispose the old socket task and create a new task with updated URI
            if (!(ohlcSocketTask.Status == TaskStatus.Running))
                return;

            ohlcSocket.StopStream();
            markSocket.StopStream();
            ohlcSocketTask = new Task( () => ohlcSocket.StartStream(wss, createOhlcRequest()));
            markSocketTask = new Task( () => markSocket.StartStream(wss, createMarkRequest()));
        }

        public string createOhlcRequest()
        {
            string sub = string.Join(",", securities);
            return $"{reqParams[0]}0{reqParams[1]}{sub}{reqParams[2]}";
        }

        public string createMarkRequest()
        {
            List<string> subs = new List<string>();
            string sub = string.Join(",", futureSecurities.Select(x => x.Key));
            Console.WriteLine(sub);
            return $"{markParams[0]}0{markParams[1]}{sub}{markParams[2]}";
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

        public Candle CreateCandle(List<string> rawData)
        {
            if (rawData.Count() != 7)
            {
                Console.WriteLine(string.Join( ",", rawData.ToArray() ));
            }
            return new Candle(new string[]
            {
                rawData[0],
                rawData[1],
                rawData[3],
                rawData[4],
                rawData[2],
                rawData[5]
            });
        }


        public async void SocketMessage(string msg)
        {
            
            var decerialized = JsonConvert.DeserializeObject<WSKline>(msg);
            if (decerialized == null) return;
            var data = decerialized.data;
            
            if (decerialized.type.Equals("welcome"))
                id = decerialized.id; // currently, I'm sending message before this even takes effect
            
            if (!decerialized.type.Equals("message"))
                return;

            string candleCode = decerialized.topic.Split(":")[1];

            switch (decerialized.subject)
            {
                case "trade.candles.update":
                    Manager.Global.UpdateLatestCandle(market, candleCode, data.candles);
                    break;

                case "trade.candles.add":
                    string uriCode = candleCode.Split("_")[0];
                    await Manager.Global.AddNewCandle(market, candleCode, uriCode, data.candles);
                    break;
                
                case "tickerV2":
                    decimal ask = decimal.Parse(data.bestAskPrice);
                    decimal bid = decimal.Parse(data.bestBidPrice);
                    Manager.Global.UpdateLatestMark(market, futureSecurities[candleCode], ask, bid);
                    break;
            }
        }
        
        private string CreateToken(string message, string secret)
        {
            secret = secret ?? "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }

        public string Result_GET(string requestPhrase)
        {
            string LocalTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();
            string str_to_sign = string.Concat(LocalTimestamp, "GET", "/api/v1/", requestPhrase);
            string signature = CreateToken(str_to_sign, api_secret);
            string passphrase = CreateToken(api_passphrase, api_secret);
            string url = string.Concat(domain, "/api/v1/", requestPhrase);
            Console.WriteLine(url);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("KC-API-SIGN", signature);
            request.Headers.Add("KC-API-TIMESTAMP", LocalTimestamp);
            request.Headers.Add("KC-API-KEY", api_key);
            request.Headers.Add("KC-API-PASSPHRASE", passphrase);
            request.Headers.Add("KC-API-KEY-VERSION", "2");

            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            return reader.ReadLine()!;
        }

        public async Task<string> Order_POST(string cli, string side, string symbol, string size, string levarage)
        {
            //Create my object
            Console.WriteLine("Placing Order");
            var myData = new
                {
                    clientOid = cli,
                    side = side,
                    symbol = symbol,
                    type = "market",
                    size = size,
                    leverage = levarage
                };

            string jsonData = JsonConvert.SerializeObject(myData);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonData);

            string LocalTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();
            string str_to_sign = string.Concat(LocalTimestamp, "POST", "/api/v1/orders", jsonData);
            string signature = CreateToken(str_to_sign, api_secret);
            string passphrase = CreateToken(api_passphrase, api_secret);
            string url = string.Concat(domain, "/api/v1/orders");
            Console.WriteLine(url);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("KC-API-SIGN", signature);
            request.Headers.Add("KC-API-TIMESTAMP", LocalTimestamp);
            request.Headers.Add("KC-API-KEY", api_key);
            request.Headers.Add("KC-API-PASSPHRASE", passphrase);
            request.Headers.Add("KC-API-KEY-VERSION", "2");

            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);
            string tempId = "";
            try
            {
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                POrderRoot decerialized = JsonConvert.DeserializeObject<POrderRoot>(reader.ReadToEnd())!;
                tempId = decerialized.data.orderId;
            }
            catch (WebException exception)
            {
                Console.WriteLine($"{exception.Message}  ...  RETRYING...");
                Thread.Sleep(1000); // Sleep for a second in case of "Too Many Requests."
            }

            if (!tempId.Equals(""))
                return tempId;

            return await Order_POST(cli, side, symbol, size, levarage);   
        }


        private string GenerateClientId()
        {
            string uniqueCLI = "";
            Random rnd = new Random();
            while (uniqueCLI.Equals(""))
            {
                string tempCLI = $"JoNaKaNy{rnd.Next()}{rnd.Next()}";
                if (orders.TryGetValue(tempCLI, out Order? order))
                    continue;
                // Once a unique client order ID has been made, exit
                uniqueCLI = tempCLI;
            }
            return uniqueCLI;
        }


        public async void PlaceOrder(Project project, string decision, bool newPos)
        {
            string pA = project.portfolio.pairA;
            string pB = project.portfolio.pairB;
            string coin = $"{pA}{pB}M";

            if (!(decision.Equals("sell") || decision.Equals("buy")))
                return;

            // Close the previous position, if any
            if (!project.clientId.Equals("") && orders.Remove(project.clientId))
            {
                await Order_POST(project.clientId, decision, coin, "1", "1"); //GenerateClientId()
                project.clientId = "";
            }
                
            if (!newPos)
                return;

            // Open a new position
            string newCli = GenerateClientId();
            string orderId = await Order_POST(newCli, decision, coin, "1", "1");
            if (orderId.Equals(""))
            {
                Console.WriteLine("OrderId IS NOTHING");
                return; // I think this happens if the account doesn't have sufficient funds.
            }

            string orderInfo = Result_GET($"orders/{orderId}");
            Console.WriteLine(orderInfo);
            var data = JsonConvert.DeserializeObject<GOrderRoot>(orderInfo)!.data;
            decimal entry = decimal.Parse(data.value) / data.size;

            // Add new order to orders dictionary and remove old one, if any
            orders.Add(newCli, new Order(entry, data.size, data.leverage, data.side));
            project.clientId = newCli;
        }


        public string GetUri(int period, int duration, string security)
        {
            long to = Helper.GetUnix();
            long from = to - duration;
            return $"{get}{uriParams[0]}{period}{uriParams[1]}{security}{uriParams[2]}{from}{uriParams[3]}{to}";
        }


        public async Task<List<Candle>> GetDataFill(string uri, int expectedCount)
        {   
            // Setup the list of candles and the amount to collect
            List<Candle> candles = new List<Candle>();

            int actualCount = 0;
            while (actualCount != expectedCount)
            {
                Thread.Sleep(1000);

                string json = await RestApi.GetJson(uri);
                var decerialized = JsonConvert.DeserializeObject<Kline>(json);

                if (decerialized == null)
                    continue;
                else if (expectedCount != -1 && decerialized.data.Count != expectedCount)
                    continue;

                // Loop through all KLines and create candles out of them
                for (int i = decerialized.data.Count - 1; i > 0; --i)
                    candles.Add(CreateCandle(decerialized.data[i]));

                actualCount = expectedCount;
            }
            return candles;
        }

        // return the candle code for the market-coin-pair
        public async Task<string> AddSecurity(Project project)
        {
            int period = project.period;
            string coin = project.portfolio.pairA;
            string pair = project.portfolio.pairB;
            string candleCode = $"{coin}-{pair}_{periods[$"{period}"]}";

            // Ensure that the coin-pair_period doesn't already exist
            if (securities.Contains(candleCode)) return "";

            // Get initial DataFill for candle data
            var candles = await GetDataFill(GetUri(period, (storageAmount * period * 60), $"{coin}-{pair}"), -1);
            if (candles.Count == 0)
                    return "";
            
            // Store the code, update project with data and add data to output file
            securities.Add(candleCode);
            futureSecurities.Add($"{coin}{pair}M", candleCode);
            Manager.Global.TryProjectFill(project, candles);
            
            // Reset existing socket task
            RestartSocketTask();

            // Start the socket task (stream will resume)
            ohlcSocketTask.Start();
            markSocketTask.Start();

            return candleCode;
        }
    }
}