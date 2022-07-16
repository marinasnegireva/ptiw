namespace Ptiw.Jobs.EventSystem
{
    public class JobMonitor : IObserver<JobCompletionData>
    {
        private readonly ILogger<JobMonitor> _logger;
        private readonly IScheduler _scheduler;
        private readonly IConfiguration _configuration;

        public JobMonitor(ILogger<JobMonitor> logger, ISchedulerFactory schedulerFactory, IConfiguration configuration)
        {
            _logger = logger;
            _scheduler = schedulerFactory.GetScheduler().Result;
            _configuration = configuration;
        }

        public async void OnCompleted()
        {
            await ReactionManager.React(string.Empty).ConfigureAwait(false);
        }

        public void OnError(Exception error)
        {
            _logger.LogError(error.ToString());
        }

        public async void OnNext(JobCompletionData value)
        {
            await ReactionManager.React(value.JobName, value.ChangesWereMade).ConfigureAwait(false);
        }

        private ReactionManager ReactionManager => new ReactionManager(_logger, _scheduler);
    }
}