using Websocket.Client;

namespace TradingBot
{
    public class WSocket
    {
        public IMarket market { get; set; }
        public WebsocketClient? client { get; set; }

        public WSocket(IMarket _m)
        {
            market = _m;
        }

        public void StopStream()
        {
            if (client == null)
                return;
            client.Dispose();
        }

        public void StartStream(string url, string request)
        {
            var uri = new Uri(url);
            client = new WebsocketClient(uri);
            ManualResetEvent exitEvent = new ManualResetEvent(false);

            using (client)
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(60);
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