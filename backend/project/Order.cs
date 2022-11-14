namespace TradingBot
{
    public class Order
    {
        public decimal entry { get; set; }
        public int size { get; set; }
        public int leverage { get; set; }
        public string side { get; set; }

        public Order(decimal _e, int _sz, int _l, string _sd)
        {
            entry = _e;
            size = _sz;
            leverage = _l;
            side = _sd;
        }
    }
}