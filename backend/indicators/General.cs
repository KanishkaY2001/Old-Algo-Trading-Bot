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
}