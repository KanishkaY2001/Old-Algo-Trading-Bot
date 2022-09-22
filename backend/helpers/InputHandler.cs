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

        private static void TryExit(string[] args, int len)
        {
            // Valid command to exit program
            if (len == 1 && args[0].Equals("e"))
            {
                active = false;
                output = "Program Exiting!";
            }
        }

        private static async Task TryAddToken(string[] args, int len)
        {
            // Invalid argument or invalid market
            if (len != 2 || !markets.Contains(args[0]))
                return;
                
            // Test to ensure that Coin-Pair is valid
            bool success = await manager.AddSecurityToMarket(args[0], args[1]);
            if (success)
                output = "Successfully added CoinPair to Market!";
            else
                output = "Invalid Coin-Pair";
        }

        private static void TryAddProject()
        {
            int index = 0;
            bool makingProject = true;
            string[] input = new string[4];
            string[] outputList = new string[]
            {
                "Project Name: ",
                "Project Market: ",
                "Project Coin: ",
                "Project Pair: "
            };

            while (makingProject)
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
                    makingProject = false;
            }

            if (manager.AddProject(input[0], input[1], input[2], input[3]))
                output = "Successfully added project!";
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
                    case "e": // Exit
                        TryExit(args, argsLen);
                        break;
                    case "new":
                        TryAddProject();
                        break;
                    case "cp": // Coin-Pair
                        TryAddToken(inputs, inputLen).Wait();
                        break;
                }

                print(output);
            }
        }
    }
}