using System.Diagnostics;

namespace TradingBot
{
    class EntryPoint
    {
        static void Main(string[] args)
        {
            var manager = Manager.Global;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start(); 

            //InputHandler.MainLoop();
            
            var project = ProcessFile.BackTest(
                "./testdata/input/Binance_ADAUSDT_2022_minute.csv",
                "./testdata/output/testFile.csv",
                // [pairA, pairB, valueA, valueB, allowance, maker, taker]
                new Portfolio("ADA","USDT",0,100,100,0.001m,0.001m),
                "1" // Period of the file (minutes in a ...)
            );
            
            
            stopWatch.Stop();
            Console.WriteLine($"Total Time: {stopWatch.ElapsedMilliseconds}");
        }
    }
}
