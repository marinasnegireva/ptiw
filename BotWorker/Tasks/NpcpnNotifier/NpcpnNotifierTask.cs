using Quartz;
using System.Text;
using Telegram.Bot;

namespace Ptiw.HostApp.Tasks.NpcpnNotifier
{
    [DisallowConcurrentExecution]
    internal class NpcpnNotifierTask : IJob
    {
        private readonly ILogger<NpcpnNotifierTask> _logger;
        private readonly ServiceContext _serviceContext;
        private readonly TelegramBotClient _telegramBotClient;
        private readonly IConfiguration _configuration;

        public NpcpnNotifierTask(ILogger<NpcpnNotifierTask> logger, ServiceContext serviceContext, IConfiguration configuration)
        {
            _logger = logger;
            _serviceContext = serviceContext;
            _configuration = configuration;
            _telegramBotClient = new TelegramBotClient(_configuration["TelegramToken"]);
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var needToNoticeAbout = _serviceContext.FindAppointmentTaskLog.Where(t => !t.Notified).ToList();
                if (!needToNoticeAbout.IsNullOrEmpty())
                {
                    var notices = needToNoticeAbout.Select(n => n.MapToNotificationData()).ToList();
                    // await _telegramBotClient.SendTextMessageAsync(SettingsManager.AppSettings.Wife_ID, CreateMessage(notices));
                    await _telegramBotClient.SendTextMessageAsync(_configuration["Husband_ID"], CreateMessage(notices));
                    _serviceContext.FindAppointmentTaskLog.Where(l => needToNoticeAbout.Select(n => n.Id).ToList().Contains(l.Id)).ToList().ForEach(l => l.Notified = true);
                    await _serviceContext.SaveChangesAsync();
                    _logger.LogInformation($"Отправил {notices.Count} посещений");
                }
                else
                {
                    _logger.LogInformation("Нет обновлений");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
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