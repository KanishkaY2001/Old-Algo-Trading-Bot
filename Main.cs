namespace TradingBot
{
    class EntryPoint
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome...");
            Candle x = new Candle(0,0,0,0,0);

            //Console.WriteLine(x.GetType().GetProperty());
            string z = "";
            var y = "ad";
            Console.WriteLine(y is string);
            z = y;
            Console.WriteLine(z);

        }
    }
}