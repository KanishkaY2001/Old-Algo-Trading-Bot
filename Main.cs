using System.Diagnostics;
using KuCoinFiles;
using Newtonsoft.Json;

namespace TradingBot
{
    class EntryPoint
    {
        static void Main(string[] args)
        {
            var manager = Manager.Global;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start(); 

            InputHandler.MainLoop();

            //var x = new KuCoin();
            //x.getDataFill("https://api.kucoin.com/api/v1/market/candles?type=1min&symbol=BAL-USDT&startAt=1667863075&endAt=1667863255");
            //Console.ReadLine();
            
            /*var project = ProcessFile.BackTest(
                "./testdata/input/BNB_ETH_USDT_1D_new.csv",
                "./testdata/output/testFile.csv",
                // [pairA, pairB, valueA, valueB, allowance, maker, taker]
                new Portfolio("ADA","USDT",0,100,100,0.001m,0.001m),
                "1440" // Period of the file (minutes in a ...)
            );*/

            
            /*ProcessFile.Optimize(
                "./testdata/input/BNB_ETH_USDT_1D_new.csv",
                "./testdata/output/testFile.csv",
                // [pairA, pairB, valueA, valueB, allowance, maker, taker]
                new Portfolio("ADA","USDT",0,100,100,0.001m,0.001m),
                "1" // Period of the file (minutes in a ...)
            );*/
            
            
            stopWatch.Stop();
            Console.WriteLine($"Total Time: {stopWatch.ElapsedMilliseconds}");
        }
    }
}
