namespace TradingBot
{
    public class RestApi
    {
        public static async Task<string> GetJson(string url)
        {
            using(var client = new HttpClient())
            {
                var endpoint = new Uri(url);
                var result = await client.GetAsync(endpoint);
                return result.Content.ReadAsStringAsync().Result;
            }
        }
    }
}