using Newtonsoft.Json;
using TradingBot;
using Kucoin.Net.Clients;
using Kucoin.Net.Enums;
using Kucoin.Net.Interfaces.Clients;
using Kucoin.Net.Objects;
using Microsoft.Extensions.Logging;

namespace KuCoinFiles
{
    public class KuCoin : IMarket
    {
        // General Information //
        public Dictionary<string,List<Candle>> securities { get; set; }
        public int storageAmount { get; set; } = 500;
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


        // Rest API Information //
        public KucoinClient client { get; set; }
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

            // Store this information somewhere private (re-factor)
            client = new KucoinClient(new KucoinClientOptions()
            {
                //ApiCredentials = new KucoinApiCredentials("634decca5777870001a98519", "27e845e3-1437-4326-884d-4aec67d5b2a1", "CleanSlate2001"),
                LogLevel = LogLevel.Trace,
                RequestTimeout = TimeSpan.FromSeconds(60),
                FuturesApiOptions = new KucoinRestApiClientOptions
                {
                    ApiCredentials = new KucoinApiCredentials("6359023df3f40e00018ae3ce", "5c6d3def-37d0-4b01-8a6d-b828b2509d44", "CleanSlate2001"),
                    AutoTimestamp = false
                }
            });
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
            latestTime = rawData[0];
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
                    if (prevCandle.Count == 0)
                        prevCandle = data.candles;

                    // Ensure that the candle doesn't already exist
                    if (prevCandle[0].Equals(latestTime))
                    {
                        break;
                    }
                    int diff = int.Parse(data.candles[0]) - int.Parse(latestTime) - 60;

                    // Missing data due to socket issue or poor network connection
                    if (diff > 120)
                    {
                        var tempUri = GetUri(diff, data.symbol);
                        var candles = await getDataFill(tempUri);
                        for (int i = 0; i < candles.Count; ++i)
                        {
                            AddCandle(candles[i], data.symbol);
                        }
                    }
                    else
                    {
                        // Update new candle has been added:
                        Candle newCandle = CreateCandle(prevCandle);
                        AddCandle(newCandle, data.symbol);
                    }
                    break;
            }
        }
        

        private async void orderHelper(OrderSide side, string coinPair)
        {
            /*
            var orderData = await client.SpotApi.Trading.PlaceOrderAsync
            (
                coinPair,
                side,
                NewOrderType.Market,
                quoteQuantity: 20
            );*/
            
            var positionResultData = await client.FuturesApi.Account.GetPositionsAsync();
            if(positionResultData.Data != null)
            {
                if (positionResultData.Data.Count() > 0)
                {
                    var y = await client.FuturesApi.Trading.PlaceOrderAsync
                    (
                        "DOGEUSDTM",
                        side,
                        NewOrderType.Market,
                        1,
                        1
                    );
                    Console.WriteLine("Y");
                    Console.WriteLine(y.Error.Message);
                }
            }
            /*
            for(int x = 0; x <= positionResultData.Data.Count(); x++)
            {
                if ("ETHUSDTM" == positionResultData.Data.ToList()[x].Symbol)
                {
                    await client.FuturesApi.Trading.PlaceOrderAsync
                    (
                        "ETHUSDTM",
                        side,
                        NewOrderType.Market,
                        0.005m,
                        1
                    );
                }
            }*/

            var x = await client.FuturesApi.Trading.PlaceOrderAsync
            (
                "DOGEUSDTM",
                side,
                NewOrderType.Market,
                1,
                1
            );

            Console.WriteLine("X");
            Console.WriteLine(x.Error.Message);
        }


        public void PlaceOrder(string decision, string coinPair)
        {
            switch(decision)
            {
                case "buy":
                    Console.WriteLine("BUYING");
                    Dummy.positionStatus = "Long";
                    orderHelper(OrderSide.Buy, coinPair);
                    break;

                case "sell":
                    Console.WriteLine("SELLING");
                    Dummy.positionStatus = "Short";
                    orderHelper(OrderSide.Sell, coinPair);
                    break;
            }
        }


        public string createRequest(List<string> subs)
        {
            string sub = string.Join(",", subs);
            return $"{reqParams[0]}0{reqParams[1]}{sub}{reqParams[2]}";
        }

        public async Task<List<Candle>> getDataFill(string uri)
        {
            // Setup the list of candles and the amount to collect
            List<Candle> candles = new List<Candle>();

            // Create url and get JSON response with GET
            string json = await RestApi.GetJson(uri);

            // Create an object from the JSON result and ensure validity
            var decerialized = JsonConvert.DeserializeObject<Kline>(json);
            if (decerialized == null || decerialized.data.Count == 0) 
                return candles;
            
            // Loop through all KLines and create candles out of them
            for (int i = decerialized.data.Count - 1; i >= 0; --i)
            {
                candles.Add(CreateCandle(decerialized.data[i]));
            }

            return candles;
        }


        public async Task<bool> AddSecurity(string p1, string p2)
        {
            string secCode = $"{p1}{code}{p2}";
            
            // Ensure that the security doesn't already exist
            if (securities.ContainsKey(secCode)) return false;
            
            // Get first datafill
            var candles = await getDataFill(GetUri((storageAmount * 60), secCode));
            if (candles.Count == 0)
                return false;

            // Create the full list of candles (previous data)
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