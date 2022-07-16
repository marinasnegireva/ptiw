using Ptiw.HostApp.Tasks.CheckNpcpnSchedule;

namespace Ptiw.Jobs.EventSystem
{
    public class ReactionManager
    {
        private readonly ILogger _logger;
        private readonly IScheduler _scheduler;

        public ReactionManager(ILogger logger, IScheduler scheduler)
        {
            _logger = logger;
            _scheduler = scheduler;
        }

        public async Task React(string jobName, bool changesWereMade = true)
        {
            _logger.LogDebug($"Reacting to {jobName} with changes({changesWereMade})");
            var reactions = ReactionList
                .Where(r => r.JobNameReactTo == jobName && (!r.NeedChangesToHappenToFire || (r.NeedChangesToHappenToFire == changesWereMade)))
                .Select(r => new Task(r.Action))
                .ToArray();

            if (!reactions.IsNullOrEmpty())
            {
                Task.WaitAll(reactions);
            }
            else
            {
                _logger.LogDebug($"No reactions to {jobName} with changes({changesWereMade})");
            }
        }

        private List<Reaction> ReactionList
        {
            get
            {
                return new List<Reaction>()
                {
                    new Reaction{JobNameReactTo = string.Empty, NeedChangesToHappenToFire = false, Action = SomeDefaultStuff },
                    new Reaction{JobNameReactTo = nameof(GetAppointmentsJob), NeedChangesToHappenToFire = true, Action = GetAppointmentsReaction },
                };
            }
        }

        private async void GetAppointmentsReaction()
        {
            _logger.LogDebug($"{nameof(GetAppointmentsReaction)} called)");
        }

        private async void SomeDefaultStuff()
        {
            _logger.LogTrace($"Default clean up event");
        }
    }
}