namespace TradingBot
{
    public interface ITask
    {
        ITask SetNext(ITask handler);
        object? HandleTask(Project project);
    }

    public abstract class TaskHandler : ITask
    {
        private ITask? nextTask;

        public ITask SetNext(ITask handler)
        {
            this.nextTask = handler;
            return handler;
        }

        public virtual object? HandleTask(Project project)
        {
            var nT = this.nextTask;
            return nT != null? nT.HandleTask(project) : null;
        }
    }
}