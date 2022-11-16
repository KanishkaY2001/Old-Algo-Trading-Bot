namespace TradingBot
{
    public class Order
    {
        public string id { get; set; }
        public decimal entry { get; set; }
        public int size { get; set; }
        public int leverage { get; set; }
        public string side { get; set; }

        public Order(string _id, decimal _e, int _sz, int _l, string _sd)
        {
            id = _id;
            entry = _e;
            size = _sz;
            leverage = _l;
            side = _sd;
        }
    }
}