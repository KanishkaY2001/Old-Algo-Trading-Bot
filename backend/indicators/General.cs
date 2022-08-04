namespace TradingBot
{
    /* Trade Responsibility Chain */
    public class TradeDecHead : TaskHandler
    {
        public override object? HandleTask(Project project)
        {
            if (project.data.Count < 2)
                return null;
                
            return base.HandleTask(project);
        }
    }

    public class TradeDecTail : TaskHandler
    {
        public override object? HandleTask(Project project)
        {
            var candle = project.data.Last();
            string prevDec = project.data[project.data.Count - 2].finalDecision;

            if ((prevDec.Equals("buy") || prevDec.Equals("hodl")) && !candle.finalDecision.Equals("sell"))
            {
                candle.finalDecision = "hodl";
            }
            return base.HandleTask(project);
        }
    }
}