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

        private static async Task<bool> TryAddProject(string[] input)
        {
            // Get array of input values and assign proper name for them
            output = "Invalid period format provided";
            string name = input[0];
            string market = input[1];
            string coin = input[2];
            string pair = input[3];
            if (!int.TryParse(input[4], out int period))
                return false;
            
            // Attempt to create the project and add it to the manager
            Random rnd = new Random();
            int num = 0;
            while (true)
            {
                name = num == 0? name : $"{name}{num}";
                if (manager.AddProject(name, market, coin, pair, period))
                    break;
                num = rnd.Next();
            }
            
            // Attempt to start the market websocket for the coin-pair
            Project project = manager.projects[name];
            output = "Invalid Market or Coin-Pair";
            for (int i = 0; i < 5; ++i)
            {
                bool success = await manager.AddSecurityToMarket(project);
                if (success)
                    break;
                else if (!success && i == 4)
                {
                    manager.RemoveProject(name);
                    return false;
                }
                    
                Thread.Sleep(2000);
            }
            
            output = "Successfully added project!";
            return true;
        }


        private static string[] TryLoadConfig(string name)
        {
            var data = new string[5];
            try 
            {
                var reader = new StreamReader($"./testdata/config/{name}");
                
                int index = -1;
                output = "Config loaded successfully!";

                using (reader)
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine()!;
                        data[++index] = line;
                    }
                }
            } 
            catch (IOException  err)
            {
                output = "File not found!";
                Console.WriteLine(err);
            }

            return data;
        }


        private static void TryStartStop(bool start)
        {
            output = "Simulation Stopped";
            if (start)
                output = "Simulation Started";
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
                        string[] input = multiInput(new string[]
                        {
                            "Project Name: ",
                            "Project Market: ",
                            "Project Coin: ",
                            "Project Pair: ",
                            "Project Period: "
                        });
                        TryAddProject(input).Wait();
                        break;
                    
                    case "load":
                        TryAddProject(TryLoadConfig(inputs[0])).Wait();
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