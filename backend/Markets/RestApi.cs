using Newtonsoft.Json;
using System.Text;

namespace TradingBot
{
    public class RestApi
    {
        class Post
        {
            public int userId { get; set; }
            public string body { get; set; }
            public string title { get; set; }
            public Post(int uid, string _b, string _t)
            {
                userId = uid;
                body = _b;
                title = _t;
            }
        }

        public static async Task<string> GetJson(string url)
        {
            using (var client = new HttpClient())
            {
                var endpoint = new Uri(url);
                var json = await client.GetAsync(endpoint);
                var result = json.Content.ReadAsStringAsync().Result;
                return result;
            }
        }

        public static async Task<string> PostJson(string url, int uid, string body, string title)
        {
            using (var client = new HttpClient())
            {
                var endpoint = new Uri(url);
                var newPost = new Post(uid, body, title);
                var newPostJs = JsonConvert.SerializeObject(newPost);
                var payload = new StringContent(newPostJs, Encoding.UTF8, "application/json");

                var json = await client.PostAsync(endpoint, payload);
                var result = json.Content.ReadAsStringAsync().Result;

                return result;
            }
        }
    }
}