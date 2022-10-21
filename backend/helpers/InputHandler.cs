namespace TradingBot
{
    public class InputHandler
    {
        private static Manager manager = Manager.Global;
        private static bool active = true;
        private static string invalid = "Invalid Input!";
        private static string output = "";
        private static string prefix = "$> ";
        private static string[] markets = new string[]
        {
            "KuCoin",
            "ByBit",
            "Binance"
        };
        
        private static void print(string input)
        {
            Console.WriteLine(input);
        }

        private static void printw(string input)
        {
            Console.Write(input);
        }

        // Used when multiple inputs are requied
        private static string[] multiInput(string[] outputList)
        {
            int index = 0;
            string[] input = new string[outputList.Length];

            while (true)
            {
                printw($"{prefix}{outputList[index]}");

                string[] args = Console.ReadLine()!.Split();
                if (args.Length > 1)
                {
                    print(invalid);
                    continue;
                }

                input[index] = args[0];
                ++index;

                if (index == input.Length)
                    break;
            }

            return input;
        }


        private static void TryExit(string[] args, int len)
        {
            // Valid command to exit program
            if (len == 1 && args[0].Equals("e"))
            {
                active = false;
                output = "Program Exiting!";
            }
        }

        private static void TryAddProject()
        {
            string[] input = multiInput(new string[]
            {
                "Project Name: ",
                "Project Market: ",
                "Project Coin: ",
                "Project Pair: ",
                "Project Period: "
            });

            if (manager.AddProject(input[0], input[1], input[2], input[3], input[4]))
                output = "Successfully added project!";
        }


        private static async Task TrySubscribe()
        {
            string[] input = multiInput(new string[]
            {
                "Market Name: ",
                "Market Coin: ",
                "Market Pair: ",
            });

            output = $"Successfully added {input[1]}-{input[2]} to {input[0]}!";
            bool success = await manager.AddSecurityToMarket(input[0], input[1], input[2]);
            if (!success)
                output = "Invalid Coin-Pair";
        }


        private static async Task TryLoadConfig(string name)
        {
            try 
            {
                var reader = new StreamReader($"./testdata/config/{name}");
                var data = new string[5];
                int index = -1;
                output = "Config loaded successfully!";

                using (reader)
                {
                    while (!reader.EndOfStream)
                    {
                        /* Get File Data */
                        var line = reader.ReadLine()!;
                        data[++index] = line;
                    }
                }

                if (!manager.AddProject(data[0], data[1], data[2], data[3], data[4]))
                {
                    output = "Invalid Project Settings!";
                }
                else
                {
                    bool suc = await manager.AddSecurityToMarket(data[1], data[2], data[3]);
                    if (!suc)
                        output = "Invalid Market Settings!";
                }
            }
            catch (IOException  err)
            {
                output = "File not found!";
                Console.WriteLine(err);
                return;
            } 
        }


        private static void TryStartStop(bool start)
        {
            output = $"Simulation Is Happening: {start}";
            manager.canPlaceOrder = start;
        }
        

        public static void MainLoop()
        {
            while (active)
            {
                printw(prefix);

                string[] args = Console.ReadLine()!.Split();
                int argsLen = args.Length;
                string command = args[0];

                string[] inputs = args.Skip(1).ToArray();
                int inputLen = inputs.Length;
                
                output = invalid;

                switch (command)
                {
                    case "e": // exit program
                        TryExit(args, argsLen);
                        break;

                    case "new": // new project
                        TryAddProject();
                        break;

                    case "sub": // sub to market | coinPair
                        TrySubscribe().Wait();
                        break;

                    case "load":
                        // projectName, marketName, pairA, pairB, periodTime
                        TryLoadConfig(inputs[0]).Wait();
                        break;

                    case "start":
                        TryStartStop(true);
                        break;

                    case "stop":
                        TryStartStop(false);
                        break;
                }
                print(output);
            }
        }
    }
}