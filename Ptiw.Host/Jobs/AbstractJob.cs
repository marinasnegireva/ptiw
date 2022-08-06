using static Ptiw.Libs.Common.Constants;

namespace Ptiw.Jobs.QuartzJobs
{
    public abstract class AbstractJob : IExpendedJob, IObservable<JobCompletionData>
    {
        public AbstractJob(ILogger<IExpendedJob> logger, IConfiguration configuration, IObserver<JobCompletionData> jobMonitor)
        {
            Logger = logger;
            Configuration = configuration;
            MonitorSubscription = Subscribe(jobMonitor);
        }

        private readonly IDisposable MonitorSubscription;
        public ILogger<IExpendedJob> Logger { get; set; }
        public IConfiguration Configuration { get; set; }
        public bool ChangesWereMade { get; set; } = false;
        private List<IObserver<JobCompletionData>> observers = new();

        public IDisposable Subscribe(IObserver<JobCompletionData> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            return new Unsubscriber(observers, observer);
        }

        public void ProcessError(Exception ex)
        {
            foreach (var observer in observers)
            {
                observer.OnError(ex);
            }
            Exit();
        }

        public void Exit()
        {
            foreach (var observer in observers)
            {
                observer.OnNext(new JobCompletionData { ChangesWereMade = ChangesWereMade, Job = GetType() });
                if(observer is ReactionManager reactionManager)
                {
                    reactionManager.WaitReactionsToComplete().Wait();
                }
            }            
            MonitorSubscription.Dispose();
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<JobCompletionData>> _observers;
            private IObserver<JobCompletionData> _observer;

            public Unsubscriber(List<IObserver<JobCompletionData>> observers, IObserver<JobCompletionData> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                foreach (var observer in _observers)
                {
                    observer.OnCompleted();
                }
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }

        public abstract Task Execute(IJobExecutionContext context);

        /// <summary>
        /// Checks env/config is job has Enabled setting, if not pauses in scheduler
        /// TODO: implement job on/off switches
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        internal bool? IsEnabled
        {
            get
            {
                if (!bool.TryParse(GetTaskData(SettingNames.Enabled), out bool enabled)) return null;
                if (!enabled)
                {
                    Logger.LogWarning("Current job is disabled in settings.");
                    return false;
                }
                return true;
            }
        }

        protected string GetTaskData(string settingName, string group = null)
        {
            var nameArray = GetType().FullName.Split(".");
            var jobName = nameArray[^2];
            var settingPath = group == null ?
                $"{SettingNames.Jobs}:{jobName}:{settingName}" : $"{SettingNames.Jobs}:{jobName}:{group}:{settingName}";
            return Configuration[settingPath];
        }
    }
}