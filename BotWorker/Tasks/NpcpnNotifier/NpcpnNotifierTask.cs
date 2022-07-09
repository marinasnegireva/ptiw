using System.Text;
using Telegram.Bot;

namespace Ptiw.HostApp.Tasks.NpcpnNotifier
{
    [DisallowConcurrentExecution]
    internal class NpcpnNotifierTask : AbstractTask
    {
        private readonly ServiceContext _serviceContext;
        private readonly TelegramBotClient _telegramBotClient;

        public NpcpnNotifierTask(ILogger<NpcpnNotifierTask> logger, ServiceContext serviceContext, IConfiguration configuration)
        {
            Logger = logger;
            Configuration = configuration;
            _serviceContext = serviceContext;
            _telegramBotClient = new TelegramBotClient(Configuration["TelegramToken"]);
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (!IsEnabled(context)) return;
                var needToNoticeAbout = _serviceContext.FindAppointmentTaskLog.Where(t => !t.Notified).ToList();
                if (!needToNoticeAbout.IsNullOrEmpty())
                {
                    var notices = needToNoticeAbout.Select(n => n.MapToNotificationData()).ToList();
                    // await _telegramBotClient.SendTextMessageAsync(SettingsManager.AppSettings.Wife_ID, CreateMessage(notices));
                    await _telegramBotClient.SendTextMessageAsync(Configuration["Husband_ID"], CreateMessage(notices));
                    _serviceContext.FindAppointmentTaskLog.Where(l => needToNoticeAbout.Select(n => n.Id).ToList().Contains(l.Id)).ToList().ForEach(l => l.Notified = true);
                    await _serviceContext.SaveChangesAsync();
                    Logger.LogInformation($"Отправил {notices.Count} посещений");
                }
                else
                {
                    Logger.LogInformation("Нет обновлений");
                }
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex.ToString());
            }
        }

        private string CreateMessage(List<NotificationData> data)
        {
            //data.Sort();
            var sb = new StringBuilder();
            sb.AppendLine("Новые доступные записи ко врачу: ");
            sb.AppendLine();
            foreach (var notification in data)
            {
                sb.AppendLine($"Дата: {notification.AppointmentDate} {notification.AppointmentDayOfWeek} Время: {notification.AppointmentTime} Врач: {notification.DoctorName}");
            }
            return sb.ToString();
        }
    }
}