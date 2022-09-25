namespace KuCoinFiles
{
    public class Kline
    {
        public string code { get; set; } = "";
        public List<List<string>> data { get; set; }

        public Kline()
        {
            code = "";
            data = new List<List<string>>();
        }
    }

    public class WSData
    {
        public string symbol { get; set; } = "";
        public List<string> candles { get; set; } = new List<string>();
        public long time { get; set; }
    }

    public class WSKline
    {
        public string id { get; set; } = "";
        public string type { get; set; } = "";
        public string topic { get; set; } = "";
        public string subject { get; set; } = "";
        public WSData data { get; set; } = new WSData();
    }
}