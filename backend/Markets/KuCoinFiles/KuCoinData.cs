namespace KuCoinFiles
{
    // KLine Data
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
        public string bestBidPrice { get; set; } = "";
        public string bestAskPrice { get; set; } = "";
    }

    public class WSKline
    {
        public string id { get; set; } = "";
        public string type { get; set; } = "";
        public string topic { get; set; } = "";
        public string subject { get; set; } = "";
        public WSData data { get; set; } = new WSData();
    }

    // POST Order data
    public class POrderData
    {
        public string orderId { get; set; } = "";
    }

    public class POrderRoot
    {
        public string code { get; set; } = "";
        public POrderData data { get; set; } = new POrderData();
    }


    // GET Order data
    public class GOrderData
    {
        public string id { get; set; } = "";
        public string symbol { get; set; } = "";
        public string type { get; set; } = "";
        public string side { get; set; } = "";
        public int size { get; set; }
        public string dealValue { get; set; } = "";
        public string value { get; set; } = "";
        public int leverage { get; set; }
    }

    public class GOrderRoot
    {
        public string code { get; set; } = "";
        public GOrderData data { get; set; } = new GOrderData();
    }


    // Token Data
    public class TokenData
    {
        public string token { get; set; } = "";
        public List<InstanceServer> instanceServers { get; set; } = new List<InstanceServer>();
    }

    public class InstanceServer
    {
        public string endpoint { get; set; } = "";
        public bool encrypt { get; set; }
        public string protocol { get; set; } = "";
        public int pingInterval { get; set; }
        public int pingTimeout { get; set; }
    }

    public class Token
    {
        public string code { get; set; } = "";
        public TokenData data { get; set; } = new TokenData();
    }

    // Active Coin Info

    public class Datum
    {
        public string symbol { get; set; } = "";
        public string baseCurrency { get; set; } = "";
        public string quoteCurrency { get; set; } = "";
        public int maxOrderQty { get; set; }
        public double maxPrice { get; set; }
        public double multiplier { get; set; }
        public double makerFeeRate { get; set; }
        public double takerFeeRate { get; set; }
        public int maxLeverage { get; set; }
    }

    public class ActiveRoot
    {
        public string code { get; set; } = "";
        public List<Datum> data { get; set; } = new List<Datum>();
    }
}