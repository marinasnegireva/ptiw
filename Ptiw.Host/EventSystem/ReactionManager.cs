using System.Collections.Concurrent;

namespace Ptiw.Host.EventSystem
{
    public partial class ReactionManager : IObserver<JobCompletionData>
    {
        private readonly ILogger _logger;
        private readonly IScheduler _scheduler;
        private readonly ServiceContext _serviceContext;
        public List<Reaction> OnCompletedReactions;
        public List<Reaction> OnNextReactions;
        public ConcurrentBag<Exception> Errors;
        private bool Reacted => OnCompletedReactions != null && OnNextReactions != null;

        public ReactionManager(ILogger logger, ISchedulerFactory schedulerFactory, ServiceContext context)
        {
            _logger = logger;
            _serviceContext = context;
            _scheduler = schedulerFactory.GetScheduler().Result;
            Errors = new ConcurrentBag<Exception>();
        }

        public async Task WaitReactionsToComplete(TimeSpan? timeout = null)
        {
            if (timeout == null) timeout = new TimeSpan(0, 1, 0);
            using var tokenSource = new CancellationTokenSource((TimeSpan)timeout);
            while (!Reacted)
            {
                if (tokenSource.IsCancellationRequested) break;
                await Task.Delay(50);
            }
        }

        public async void OnCompleted()
        {
            OnCompletedReactions = await React(null);
        }

        public void OnError(Exception error)
        {
            Errors.Add(error);
        }

        public async void OnNext(JobCompletionData value)
        {
            OnNextReactions = await React(value.Job, value.ChangesWereMade);
        }

        private async Task<List<Reaction>> React(Type? job, bool changesWereMade = true)
        {
            _logger.LogDebug($"Reacting to {job} with changes({changesWereMade})");
            var reactions = ReactionList
                .Where(r => r.JobReactTo == job && (!r.NeedChangesToHappenToFire || (r.NeedChangesToHappenToFire == changesWereMade)))
                .ToList();
            if (!reactions.IsNullOrEmpty())
            {
                await TasksExecute(reactions);
            }
            else
            {
                _logger.LogDebug($"No reactions to {job} with changes({changesWereMade})");
            }
            return reactions;
        }

        private async Task TasksExecute(List<Reaction> reactions)
        {
            reactions.ForEach(r => r.Task = Task.Run(r.Action));
            var tasks = reactions.Select(r => r.Task).ToArray();
            await Task.WhenAll(tasks);
            tasks.Where(t => t.IsFaulted && t.Exception != null).ToList().ForEach(t => Errors.Add(t.Exception));
        }

        private List<Reaction> ReactionList
        {
            get
            {
                var result = new List<Reaction>();
                result.AddRange(ClinicChain);

                result.AddRange(new List<Reaction>()
                {
                    new Reaction{
                        JobReactTo = null,
                        NeedChangesToHappenToFire = false,
                        Action = SomeDefaultStuff },
                });
                return result;
            }
        }

        //private async void GetAppointmentsReaction()
        //{
        //    try
        //    {
        //        _logger.LogDebug($"{nameof(GetAppointmentsReaction)} called)");
        //    }
        //    catch(Exception ex)
        //    {
        //        Errors.Add(ex);
        //    }

        //}

        private async void SomeDefaultStuff()
        {
            try
            {
                _logger.LogTrace($"Default clean up event");
            }
            catch (Exception ex)
            {
                Errors.Add(ex);
            }
        }
    }
}