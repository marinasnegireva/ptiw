using Ptiw.Jobs.QuartzJobs;
using Ptiw.Libs.EF.Tables;
using Telegram.Bot;
using static Ptiw.Libs.Common.Constants;

namespace Ptiw.HostApp.Tasks.NpcpnNotifier
{
    [DisallowConcurrentExecution]
    public class NotifierJob : AbstractJob
    {
        private readonly ServiceContext _serviceContext;
        private readonly TelegramBotClient _telegramBotClient;

        public NotifierJob(ILogger<NotifierJob> logger, ServiceContext serviceContext, IConfiguration configuration, JobMonitor jobMonitor, IValidator<NotifierJob> validator)
            : base(logger, configuration, jobMonitor)
        {
            validator.ValidateAndThrow(this);
            _serviceContext = serviceContext;
            _telegramBotClient = new TelegramBotClient(configuration[SettingNames.TelegramToken]);
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (IsEnabled == false)
                {
                    await context.Scheduler.PauseJob(context.JobDetail.Key);
                    return;
                }

                var needToNoticeAbout = _serviceContext.Notifications.Where(t => !t.Completed).ToList() ?? new List<Notification>();
                if (!needToNoticeAbout.IsNullOrEmpty())
                {
                    await SendEveryNotificationAsync(needToNoticeAbout);
                    await ChangeCompletionStatus(needToNoticeAbout);
                    ChangesWereMade = true;
                }
                else
                {
                    Logger.LogInformation("No notifications.");
                }
                Exit();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex.ToString());
            }
        }

        public async Task SendEveryNotificationAsync(List<Notification> notifications)
        {
            Logger.LogInformation($"Sending {notifications.Count} notifications");
            var tasks = notifications.Select(n => _telegramBotClient.SendTextMessageAsync(n.UserIdTo, n.NotificationText)).ToList();
            while (tasks.Any())
            {
                var finishedTask = await Task.WhenAny(tasks);
                tasks.Remove(finishedTask);
                if (!finishedTask.IsCompletedSuccessfully)
                {
                    Logger.LogError($"Task for SendTextMessageAsync failed. Error: {finishedTask.Exception}");
                }
            }
            Logger.LogDebug("Sent.");
        }

        private async Task ChangeCompletionStatus(List<Notification> notifications)
        {
            Logger.LogInformation("Saving notifications status info.");
            notifications.ForEach(n => n.CompletedTime = DateTime.UtcNow);
            notifications.ForEach(n => n.Completed = true);
            _serviceContext.Notifications.UpdateRange(notifications);
            await _serviceContext.SaveChangesAsync();
            Logger.LogDebug($"Changed saved.");
        }         
    }
}