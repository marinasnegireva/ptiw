using Ptiw.Jobs.QuartzJobs;
using Ptiw.Libs.EF.Tables;
using System.Text;

namespace Ptiw.Host.Jobs.Clinic.FindAppointmentsForUser
{
    /// <summary>
    /// Finds appointments for specific user configs and created notification data
    /// </summary>
    public class Job : AbstractJobWithConfigForUser<FindAppointmentsConfig>
    {
        private readonly ServiceContext _serviceContext;
        private List<NpcpnAppointment> _finalResult;
        private readonly IValidator<Job> _validator;

        public Job(ILogger<IExpendedJob> logger, ServiceContext serviceContext, IConfiguration configuration,
            IObserver<JobCompletionData> jobMonitor, IValidator<Job> validator)
            : base(logger, configuration, jobMonitor)
        {
            _serviceContext = serviceContext;
            _finalResult = new List<NpcpnAppointment>();
            _validator = validator;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Logger.LogDebug("Started!");

                SetTaskConfiguration(context);
                _validator.ValidateAndThrow(this);

                _finalResult = _serviceContext.GetAppointmentsUserWasntNotifiedAbout(TaskConfiguration.UserId, GetType());
                FilterAppointments();
                await SaveResults();
                Exit();
            }
            catch (Exception ex)
            {
                ProcessError(ex);
            }
        }

        private async Task SaveResults()
        {
            if (_finalResult.Any())
            {
                Logger.LogDebug($"Saving notification for {_finalResult.Count} appointments for User {TaskConfiguration.UserId}.");
                await _serviceContext.Notifications.AddAsync(new Notification { Added = DateTime.UtcNow, UserIdTo = TaskConfiguration.UserId, NotificationText = CreateMessage() });
                await _serviceContext.AddDataToNotificationLog(_finalResult.Select(fr => fr.Id).ToList(), TaskConfiguration.UserId, GetType(), false);
                await _serviceContext.SaveChangesAsync();
                Logger.LogInformation("Saved.");
                ChangesWereMade = true;
            }
        }

        private void FilterAppointments()
        {
            Logger.LogInformation($"Found {_finalResult.Count} appointments. Filtering.");
            var validator = new NpcpnAppointmentValidator();
            var filteredResult = new List<NpcpnAppointment>();
            foreach (var result in _finalResult)
            {
                result.TaskConfigForFiltering = TaskConfiguration;
                var validationResult = validator.Validate(result);
                if (validationResult.IsValid)
                {
                    filteredResult.Add(result);
                }
            }
            _finalResult = filteredResult;
            Logger.LogInformation($"Filtered to {_finalResult.Count} appointments.");
        }

        private string CreateMessage()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Новые доступные записи ко врачу: ");
            sb.AppendLine();
            foreach (var notification in _finalResult)
            {
                sb.AppendLine($"Дата: {notification.Appointment.Month}.{notification.Appointment.Day} " +
                    $"{Constants.RuCulture.DateTimeFormat.GetDayName(notification.Appointment.DayOfWeek)} " +
                    $"Время: {notification.Appointment.Hour}:{notification.Appointment.Minute} Врач: {notification.DoctorName}");
            }
            return sb.ToString();
        }
    }
}