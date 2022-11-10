using Newtonsoft.Json;
using TradingBot;
using Kucoin.Net.Enums;
using System.Security.Cryptography;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace KuCoinFiles
{
    public class KuCoin : IMarket
    {
        // General Information //
        public Dictionary<string,List<Candle>> securities { get; set; }
        public int storageAmount { get; set; } = 100;
        public string code { get; set; } = "-";
        public string market { get; set; } = "KuCoin";
        public string latestTime { get; set; } = "";
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
        public bool orderPlaced = false;



        // Websocket information //
        public string wss { get; set; } = "";
        public static string id { get; set; } = "";
        public WSocket socket { get; set; }
        public Task socketTask { get; set; }
        public List<string> prevCandle { get; set; } = new List<string>();
        public string[] reqParams { get; set; } = new string[]
        {
            "{ \"id\": ", // followed by id
            ",\"type\": \"subscribe\", \"topic\": \"/market/candles:", // followed by string.Join(",", subs)
            "\", \"privateChannel\": false, \"response\": true}"
        };

        // have this for every token.
        // This is important because sometimes, after adding a new token, there may not be a trade.candles.update, which means prevCandle is
        // not updated to what it should be. THis happens for coins which are not popular. If the initialUpdate doesn't happen, then we need
        // a datafill compulsory. After the datafill, the initialUpdate has happened. Think of this like a one-off, that's required for each token.
        public bool initialUpdate = false;


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

            socketTask = new Task
            ( 
                () => socket.StartStream(wss, createRequest(socket.subs))
            );
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
            Console.WriteLine($"Previous Candle Time / Latest Time: {latestTime}");
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
            
            /*
            if (decerialized.type.Equals("welcome"))
            {
                id = decerialized.id; // currently, I'm sending message before this even takes effect
            }
            */
            

            switch (decerialized.subject)
            {
                case "trade.candles.update":
                    Console.WriteLine($"PrevCandle updated: {prevCandle[0]}");
                    prevCandle = data.candles;
                    if (!initialUpdate) initialUpdate = true;
                    break;

                case "trade.candles.add":
                    // Latest candle's unix time.
                    Console.WriteLine(data.candles[0]);

                    int diff = int.Parse(data.candles[0]) - int.Parse(latestTime);
                    Console.WriteLine(diff);

                    // Missing data due to socket issue or poor network connection
                    if (diff > 120 || !initialUpdate)
                    {
                        if (!initialUpdate) initialUpdate = true;
                        Console.WriteLine("Data fill required.");
                        var tempUri = GetUri(diff, data.symbol);
                        Console.WriteLine(tempUri);

                        var candles = await getDataFill(tempUri, diff/60);
                        for (int i = 0; i < candles.Count; ++i)
                        {
                            AddCandle(candles[i], data.symbol);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Adding new candle. No fill required.");
                        Console.WriteLine(prevCandle[0]);
                        latestTime = prevCandle[0];
                        // Update new candle has been added:
                        Candle newCandle = CreateCandle(prevCandle);
                        AddCandle(newCandle, data.symbol);
                    }

                    break;
            }

            /*
            var decerialized = JsonConvert.DeserializeObject<WSKline>(msg);
            if (decerialized == null)
                return;
            var data = decerialized.data;

            if (decerialized.type.Equals("welcome"))
            {
                id = decerialized.id; // currently, I'm sending message before this even takes effect
            }
            
            switch (decerialized.subject)
            {
                case "trade.candles.update":
                    prevCandle = data.candles; // Only works for a single coin-pair, (1 market = 1 coin-pair) , need ot make this a list to add more
                    Manager.Global.UpdateProjects(null, market, data.symbol);
                    break;

                case "trade.candles.add":
                    // Ensure that a previous candle is available
                    Console.WriteLine("ADDING..............");

                    //if (prevCandle.Count == 0)
                    //    prevCandle = data.candles;

                    Console.WriteLine(prevCandle[0]);

                    // Ensure that the candle doesn't already exist
                    if (data.candles[0].Equals(latestTime))
                    {
                        // This means that the current candle that came through the socket has same unix time
                        // as the prevCandle. We want to avoid this to not have duplicate entries.
                        Console.WriteLine("LATEST TIME ERROR");
                        break;
                    }
                    
                    int diff = int.Parse(data.candles[0]) - int.Parse(latestTime);
                    Console.WriteLine(diff);
                    // Missing data due to socket issue or poor network connection
                    if (diff > 60)
                    {
                        Console.WriteLine("Data fill required.");
                        var tempUri = GetUri(diff, data.symbol);
                        var candles = await getDataFill(tempUri);
                        for (int i = 0; i < candles.Count; ++i)
                        {
                            Console.WriteLine($"Adding candle: {Helper.UnixToDate(candles[i].unix)}");
                            AddCandle(candles[i], data.symbol);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Adding new candle. No fill required.");
                        // Update new candle has been added:
                        Candle newCandle = CreateCandle(prevCandle);
                        AddCandle(newCandle, data.symbol);
                    }
                    break;
            }
            */
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

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("KC-API-SIGN", signature);
            request.Headers.Add("KC-API-TIMESTAMP", LocalTimestamp);
            request.Headers.Add("KC-API-KEY", api_key);
            request.Headers.Add("KC-API-PASSPHRASE", passphrase);
            request.Headers.Add("KC-API-KEY-VERSION", "2");

            WebResponse response = request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string str = reader.ReadLine();
            return str;
        }

        public string Order_POST(string side, string symbol, string size, string levarage)
        {
            //Create my object
            var myData = new
                {
                    clientOid = @"myownID1234",
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

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("KC-API-SIGN", signature);
            request.Headers.Add("KC-API-TIMESTAMP", LocalTimestamp);
            request.Headers.Add("KC-API-KEY", api_key);
            request.Headers.Add("KC-API-PASSPHRASE", passphrase);
            request.Headers.Add("KC-API-KEY-VERSION", "2");
            Console.WriteLine(passphrase);
            Console.WriteLine(signature);
            Console.WriteLine(url);

            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);

            WebResponse response = request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string x = reader.ReadToEnd();
            return x;
        }


        public void PlaceOrder(string decision, string coinPair)
        {
            string coin = "BALUSDTM";

            switch(decision)
            {
                case "buy":
                    Console.WriteLine("BUYING");
                    if (orderPlaced)
                        JObject.Parse(Order_POST("buy", coin, "5", "2"));
                        
                    JObject.Parse(Order_POST("buy", coin, "5", "2"));
                    Console.WriteLine("Bought");
                    orderPlaced = true;
                    break;

                case "sell":
                    Console.WriteLine("SELLING");
                    if (orderPlaced)
                        JObject.Parse(Order_POST("sell", coin, "5", "2"));
                    JObject.Parse(Order_POST("sell", coin, "5", "2"));
                    Console.WriteLine("Sold");
                    orderPlaced = true;
                    break;
            }
        }


        public string createRequest(List<string> subs)
        {
            string sub = string.Join(",", subs);
            return $"{reqParams[0]}0{reqParams[1]}{sub}{reqParams[2]}";
        }


        public async Task<List<Candle>> getDataFill(string uri, int expectedCount)
        {   
            // Setup the list of candles and the amount to collect
            List<Candle> candles = new List<Candle>();

            int actualCount = 0;
            while (actualCount != expectedCount)
            {
                Thread.Sleep(1000);

                string json = await RestApi.GetJson(uri);
                var decerialized = JsonConvert.DeserializeObject<Kline>(json);

                Console.WriteLine($"Current Count: {decerialized.data.Count}");

                if (expectedCount != -1)
                    if (decerialized == null || decerialized.data.Count != expectedCount)
                        continue;

                // Loop through all KLines and create candles out of them
                for (int i = decerialized.data.Count - 1; i > 0; --i)
                {
                    latestTime = decerialized.data[i][0];
                    prevCandle = decerialized.data[i];
                    candles.Add(CreateCandle(decerialized.data[i]));
                }
                actualCount = expectedCount;
            }

            return candles;
        }


        public async Task<bool> AddSecurity(string p1, string p2)
        {
            string secCode = $"{p1}{code}{p2}";
            
            // Ensure that the security doesn't already exist
            if (securities.ContainsKey(secCode)) return false;
            
            // Get first datafill
            var candles = await getDataFill(GetUri((storageAmount * 60), secCode), -1);
            if (candles.Count == 0)
                return false;

            // Create the full list of candles (previous data)

            // Remove the latest candle because it's not finished yet
            //candles.RemoveAt(candles.Count - 1);

            securities.Add(secCode, candles);
            
            // Update project with data and add data to output file
            UpdateManager(null!, secCode);

            // Start websocket stream
            socket.subs.Add($"{secCode}_{periods["1"]}");

            // Start the socket task
            socketTask.Start();
            return true;
        }
    }
}