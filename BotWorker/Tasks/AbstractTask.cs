namespace Ptiw.HostApp.Tasks
{
    public abstract class AbstractTask : IExpendedJob
    {
        public ILogger<IJob> Logger { get; set; }
        public IConfiguration Configuration { get; set; }

        public abstract Task Execute(IJobExecutionContext context);

        /// <summary>
        /// Checks env/config is job has Enabled setting, if not pauses in scheduler
        /// TODO: implement job on/off switches
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        protected bool IsEnabled(IJobExecutionContext context)
        {
            if (!bool.TryParse(GetTaskData("Enabled"), out bool enabled)) throw new ArgumentException("Cannot parse if task is Enabled");
            if (!enabled)
            {
                context.Scheduler.PauseJob(context.JobDetail.Key);
                return false;
            }
            return true;
        }

        protected string GetTaskData(string settingName, string group = null)
        {
            var jobName = GetType().Name;
            var settingPath = group == null ? $"Tasks:{jobName}:{settingName}" : $"Tasks:{jobName}:{group}:{settingName}";
            return Configuration[settingPath];
        }
    }
}