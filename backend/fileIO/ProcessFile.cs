namespace TradingBot
{
    public class ProcessFile
    {
        public static Project BackTest(string _in, string _out, Portfolio portfolio, int skip, int _p)
        {
            var reader = new StreamReader(_in);
            var writer = new StreamWriter(_out, false);
            var tradeHead = Manager.Global.tradeDecHead;
            var project = new Project(tradeHead, portfolio, "", "test", _p, true);

            using (reader) using (writer)
            {
                reader.ReadLine(); // skip first line
                writer.WriteLine(GenerateHeader());

                var data = project.data;
                var snapshots = project.snapshots;

                while (!reader.EndOfStream)
                {
                    /* Determines if the candle can buy / sell */
                    bool canProcess = data.Count >= skip ? true : false;

                    /* Get File Data */
                    var line = reader.ReadLine()!;

                    /* Process Candle */
                    var candle = new Candle(line.Split(","));
                    project.ProcessCandle(candle, canProcess);
                }

                for (int i = 0; i < data.Count; ++i)
                {
                    snapshots.TryGetValue(data[i].unix, out var snapshot);
                    writer.WriteLine($"{CandleToString(data[i])}{snapshot}");
                }
            }

            return project;
        }


        public static void Optimize(string _in, string _out, Portfolio portfolio, string _p)
        {
            var reader = new StreamReader(_in);
            //var writer = new StreamWriter(_out, false);
            var data = new List<String>();
            var projects = new List<Project>();

            using (reader)
            {
                reader.ReadLine(); // skip first line

                while (!reader.EndOfStream)
                {
                    /* Get File Data */
                    var line = reader.ReadLine()!;

                    /* Process Candle */
                    data.Add(line);
                }
            }

            var chandOpts = new List<ChandelierOpt>();
            for (int i = 17; i < 27; ++i) // 17 - 27 = 10
            {
                for (int j = 10; j < 60; j+=5) // 1 - 5 = 10
                {
                    chandOpts.Add(new ChandelierOpt(i, j/10m));
                }
            }
            
            var tradeHead = Manager.Global.tradeDecHead;
            Parallel.For(0, chandOpts.Count, i => {
                var project = new Project(tradeHead, new Portfolio("ADA","USDT",0,100,100,0.001m,0.001m), "", $"test{i}", 1440, true);
                project.chandalierOpt = chandOpts[i];
                for (int j = 0; j < data.Count; ++j)
                {
                    project.ProcessCandle(new Candle(data[j].Split(",")), true);
                }
                projects.Add(project);
                //Console.WriteLine(project.portfolio.allProfit);
            });

            
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
            header = $"{header},swingDec,ChEDec"; // Chandalier Exit
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

            /* Swing Arm Info */
            output = $"{output}{candle.swingDecision},";

            /* Chandalier Info */
            output = $"{output}{candle.chandDecision},";

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