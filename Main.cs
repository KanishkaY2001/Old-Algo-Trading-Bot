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
                "./testdata/input/BNB_ETH_USDT_1H.csv",
                "./testdata/output/testFile.csv",
                // [pairA, pairB, valueA, valueB, allowance, maker, taker]
                new Portfolio("ETH","USDT",0,100,100,0.001m,0.001m)
            );
            
            

            stopWatch.Stop();
            Console.WriteLine(stopWatch.ElapsedMilliseconds);
        }
    }
}