using Websocket.Client;

namespace TradingBot
{
    public class WSocket
    {
        public List<string> subs { get; set; } = new List<string>();
        public IMarket market { get; set; }

        public WSocket(IMarket _m)
        {
            market = _m;
        }

        public void StartStream(string url, string request)
        {
            var exitEvent = new ManualResetEvent(false);
            var uri = new Uri(url);

            using (var client = new WebsocketClient(uri))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);

                client.ReconnectionHappened.Subscribe
                (info => 
                    {
                        Console.WriteLine($"Issue at: {Helper.UnixToDate(Helper.GetUnix())}" );
                        if (info.Type != ReconnectionType.Initial)
                        {
                            Console.WriteLine(($"Reconnection happened, type: {info.Type}"));
                            client.Send(request);
                        }
                    }
                );

                client.MessageReceived.Subscribe
                (msg => 
                    {
                        market.SocketMessage($"{msg}");
                        //Console.WriteLine(($"{msg}"));
                    }
                );
                
                client.Start();
                client.Send(request);
                exitEvent.WaitOne();
            }

        }
    }
}