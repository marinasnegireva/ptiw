namespace Ptiw.Jobs.QuartzJobs
{
    public abstract class AbstractJobWithConfigForUser<TConfig> : AbstractJob
    {
        public AbstractJobWithConfigForUser(ILogger<IExpendedJob> logger, IConfiguration configuration, IObserver<JobCompletionData> jobMonitor)
            : base(logger, configuration, jobMonitor)
        {
        }

        public TConfig TaskConfiguration { get; set; }

        protected void SetTaskConfiguration(IJobExecutionContext context)
        {
            var configDataString = context.JobDetail.JobDataMap.Get(nameof(TConfig)).ToString();
            if (string.IsNullOrEmpty(configDataString)) throw new ArgumentException($"Configuration {nameof(TConfig)} in task data map is empty");
            TaskConfiguration = JsonConvert.DeserializeObject<TConfig>(configDataString);
            if (TaskConfiguration == null) throw new ArgumentException($"Configuration data {configDataString} did not deserialize correctly");
        }
    }
}