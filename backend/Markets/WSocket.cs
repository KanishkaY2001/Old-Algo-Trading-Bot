using Websocket.Client;

namespace TradingBot
{
    public class WSocket
    {
        public static void Test(string url, string _cp)
        {
            var exitEvent = new ManualResetEvent(false);
            var uri = new Uri(url);
            var req = "{\"topic\":\"trade\",\"params\":{\"symbol\":\"" + _cp + "\",\"binary\":false},\"event\":\"sub\"}";
            Console.WriteLine(req);
            using (var client = new WebsocketClient(uri))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info =>
                    Console.WriteLine(($"Reconnection happened, type: {info.Type}")));

                client.MessageReceived.Subscribe(msg => Console.WriteLine(($"MSG: {msg}")));
                client.Start();

                Task.Run(() => client.Send(req));

                exitEvent.WaitOne();
            }

        }
    }
}