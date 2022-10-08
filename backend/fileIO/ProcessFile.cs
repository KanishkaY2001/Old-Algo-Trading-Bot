namespace TradingBot
{
    public class ProcessFile
    {
        public static Project BackTest(string _in, string _out, Portfolio portfolio)
        {
            var reader = new StreamReader(_in);
            var writer = new StreamWriter(_out, false);
            var tradeHead = Manager.Global.tradeDecHead;
            var project = new Project(tradeHead, portfolio, "", "", "test", "60");

            using (reader) using (writer)
            {
                reader.ReadLine(); // skip first line
                writer.WriteLine(GenerateHeader());

                var data = project.data;
                var snapshots = project.snapshots;

                while (!reader.EndOfStream)
                {
                    /* Get File Data */
                    var line = reader.ReadLine()!;

                    /* Process Candle */
                    var candle = new Candle(line.Split(","));
                    project.ProcessCandle(candle, true);
                    Console.WriteLine(line);
                }

                for (int i = 0; i < data.Count; ++i)
                {
                    snapshots.TryGetValue(data[i].unix, out var snapshot);
                    writer.WriteLine($"{CandleToString(data[i])}{snapshot}");
                }
            }

            return project;
        }


        public static void ProcessAll(string _out, Project project)
        {
            var writer = new StreamWriter(_out, true);
            using (writer)
            {
                writer.WriteLine(GenerateHeader());
                for (int i = 0; i < project.data.Count; ++i)
                {
                    writer.WriteLine(CandleToString(project.data[i]));
                }
                
            }
            writer.Close();
        }

        public static void ProcessNext(string _out, Project project)
        {
            var writer = new StreamWriter(_out, true);
            var data = project.data;
            int idx = data.Count - 1;

            using (writer)
            {
                project.snapshots.TryGetValue(data[idx].unix, out var snapshot);
                writer.WriteLine($"{CandleToString(data[idx])}{snapshot}");
            }
            writer.Close();
        }


        /* Create File Header */
        public static string GenerateHeader()
        {
            string header = "";
            header = "Date,Time,Open,High,Low,Close,FinalDecision"; // general candle
            header = $"{header},HiLo,HiLoDec"; // HiLo indicator
            header = $"{header},Macd,Signal,Histogram,Crossover,MacdDec"; // Macd indicator
            header = $"{header},Rsi,RsiMa"; // Rsi Indicator
            header = $"{header},Stoch_K,Stoch_D"; // Stoch Rsi Indicator
            header = $"{header},pairA,pairB,Profit,AllProfit"; // Portfolio
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
            output = $"{output}{candle.cross},";

            //output = $"{output}{}"
            output = $"{output}{candle.macDecision},";

            /* RelStrIdx Info */
            output = $"{output}{candle.rsi:0.##},";
            output = $"{output}{candle.rsiMA:0.##},";

            /* Stoch Rsi Info */
            output = $"{output}{candle.K:0.##},";
            output = $"{output}{candle.D:0.##},";

            return output;
        }
    }
}