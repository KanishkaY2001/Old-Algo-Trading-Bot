using System.Diagnostics;

namespace TradingBot
{
    public class ProcessFile
    {
        public static Project BackTest(string _in, string _out, Portfolio portfolio)
        {
            var reader = new StreamReader(_in);
            var writer = new StreamWriter(_out, false);
            var project = new Project(Manager.Global.tradeDecHead, portfolio);

            using (reader) using (writer)
            {
                reader.ReadLine(); // skip first line
                var portfolioSnaps = new List<string>();

                writer.WriteLine(GenerateHeader());
                var data = project.data;

                while (!reader.EndOfStream)
                {
                    /* Get File Data */
                    var line = reader.ReadLine()!;

                    /* Process Candle */
                    var candle = new Candle(line.Split(","));
                    project.ProcessCandle(candle);

                    /* Process File Writing */
                    portfolioSnaps.Add(PortfolioToString(project));
                    if (data.Count == project.maxDataLen + 1)
                        writer.WriteLine($"{CandleToString(data[0])}{portfolioSnaps.Last()}");
                }

                for (int i = 0; i < data.Count; ++i)
                {
                    int snapIdx = portfolioSnaps.Count - (project.maxDataLen + 1 - i);
                    writer.WriteLine($"{CandleToString(data[i])}{portfolioSnaps[snapIdx]}");
                }
            }

            return project;
        }

        /* Create File Header */
        public static string GenerateHeader()
        {
            string header = "";
            header = "Date,Time,Open,High,Low,Close,FinalDecision"; // general candle
            header = $"{header},HiLo,HiLoDec"; // HiLo indicator
            header = $"{header},Macd,Signal,Histogram,MacdDec"; // Macd indicator
            header = $"{header},Rsi,RsiMa"; // Rsi Indicator
            header = $"{header},pairA,pairB,Revenue,Loss,Profit,+Trades,-Trades"; // Portfolio
            return header;
        }

        /* Get Candle Data */
        public static string CandleToString(Candle candle)
        {
            string output = "";
            
            /* General Info */
            output = $"{Helper.UnixToDate(candle.unix)},";
            output = $"{output}{candle.unix},";
            output = $"{output}{candle.open},";
            output = $"{output}{candle.high},";
            output = $"{output}{candle.low},";
            output = $"{output}{candle.close},";
            output = $"{output}{candle.finalDecision},";

            /* HighLow Info */
            output = $"{output}{candle.hilo},";
            output = $"{output}{candle.hiloDecision},";

            /* MacdSig Info */
            output = $"{output}{(candle.macd != null? $"{candle.macd:0.####}" : "")},";
            output = $"{output}{(candle.signal != null? $"{candle.signal:0.####}" : "")},";
            output = $"{output}{(candle.hist != null? $"{candle.hist:0.####}" : "")},";
            output = $"{output}{candle.macDecision},";

            /* RelStrIdx Info */
            output = $"{output}{candle.rsi:0.##},";
            output = $"{output}{candle.rsiMA:0.##},";

            return output;
        }

        /* Portfolio Data to String */
        public static string PortfolioToString(Project project)
        {
            string output = "";
            var p = project.portfolio;
            decimal lr = p.loss / p.allowance;
            output = $"{p.valueA:0.###},";
            output = $"{output}{p.valueB:0.###},";
            output = $"{output}{p.revenueRate:0.###},";
            output = $"{output}{lr:0.###},";
            output = $"{output}{(p.revenueRate - lr):0.###},";
            output = $"{output}{p.goodTrades},";
            output = $"{output}{p.badTrades}";
            return output;
        }
    }
}