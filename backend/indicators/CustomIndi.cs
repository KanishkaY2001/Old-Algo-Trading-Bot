
/*namespace TradingBot
{
    public class CustomIndi
    {
        public static void ApplyIndicator(Project project)
        {
            var data = project.data;
            Candle candle = data[data.Count() - 1];
            var idlehodl = candle.finalDecision.Equals("idle") || candle.finalDecision.Equals("hodl");
            if (!idlehodl)
                return;

            var chand = candle.chandDecision;
            decimal macd = 0;
            decimal sig = 0;
            var prevCross = "-";
            for (int i = data.Count() - 1; i >= 0; --i)
            {
                if (!data[i].cross.Equals("-") && prevCross.Equals("-"))
                {
                    prevCross = data[i].cross;
                    macd = data[i].macd != null ? (decimal)data[i].macd! : 0;
                    sig = data[i].signal != null ? (decimal)data[i].signal! : 0;

                    if ((macd < 0 || sig < 0) && (prevCross.Equals("green")) && (chand.Equals("buy")))
                        candle.finalDecision = "buy";
                    else if ((prevCross.Equals("red") && macd < 0) || (chand.Equals("sell")))
                        candle.finalDecision = "sell";
                    else if ((macd > 0 || sig > 0) && (prevCross.Equals("red")) && chand.Equals("sell"))
                        candle.finalDecision = "buy";
                    else if ((prevCross.Equals("green") && macd > 0) || chand.Equals("buy"))
                        candle.finalDecision = "sell";
                    // I think there is a major bug here because it's not a trade decision, there are multiple sells that happen
                    return;
                }
            }            
        }
    }
}*/