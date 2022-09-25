namespace KuCoinFiles
{
     public class Data
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
        public Data data { get; set; } = new Data();
    }
}