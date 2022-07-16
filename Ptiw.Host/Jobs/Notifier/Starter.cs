namespace Ptiw.Host.Jobs.Notifier
{
    public static class Starter
    {
        private static DateTime LastTimeStarted = DateTime.Now;

        public static async Task Start(IScheduler scheduler)
        {
            if (LastTimeStarted.AddMinutes(2) > DateTime.Now)
            {
                var job = JobBuilder.Create<Job>()
                        .WithIdentity(typeof(Job).FullName)
                        .Build();

                await scheduler.AddJob(job, true);
                await scheduler.TriggerJob(job.Key);
                LastTimeStarted = DateTime.Now;
            }
        }
    }
}