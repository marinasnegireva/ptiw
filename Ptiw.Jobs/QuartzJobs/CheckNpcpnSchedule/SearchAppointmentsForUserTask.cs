using Ptiw.Jobs.QuartzJobs;
using Ptiw.Libs.EF.Tables;
using Ptiw.Libs.Validation.Validators;
using System.Text;

namespace Ptiw.HostApp.Tasks.CheckNpcpnSchedule
{
    /// <summary>
    /// Finds appointments for specific user configs and created notification data
    /// </summary>
    public class SearchAppointmentsForUserJob : AbstractJobWithConfigForUser<SearchAppointmentsForUserConfig>
    {
        private readonly ServiceContext _serviceContext;
        private List<NpcpnAppointment> _finalResult;

        public SearchAppointmentsForUserJob(ILogger<SearchAppointmentsForUserJob> logger, ServiceContext serviceContext, IConfiguration configuration,
            JobMonitor jobMonitor, IValidator<SearchAppointmentsForUserJob> validator)
            : base(logger, configuration, jobMonitor)
        {
            validator.ValidateAndThrow(this);
            _serviceContext = serviceContext;
            _finalResult = new List<NpcpnAppointment>();
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Logger.LogDebug("Started!");

                SetTaskConfiguration(context);

                var appointmentsToProcess = _serviceContext.GetAppoitmentsUserWasntNotifiedAbout(TaskConfiguration.UserId, nameof(SearchAppointmentsForUserJob));
                FilterAppointments();
                await SaveResults();
                Exit();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex.ToString());
            }
        }

        private async Task SaveResults()
        {
            if (_finalResult.Any())
            {
                Logger.LogDebug($"Saving notification for {_finalResult.Count} appointments for User {TaskConfiguration.UserId}.");
                await _serviceContext.Notifications.AddAsync(new Notification { Added = DateTime.UtcNow, UserIdTo = TaskConfiguration.UserId, NotificationText = CreateMessage() });
                await _serviceContext.AddDataToNotificationLog(_finalResult.Select(fr => fr.Id).ToList(), TaskConfiguration.UserId, nameof(SearchAppointmentsForUserJob), false);
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
                sb.AppendLine($"Дата: {notification.AppointmentDate} {notification.AppointmentDayOfWeek} Время: {notification.AppointmentTime} Врач: {notification.DoctorName}");
            }
            return sb.ToString();
        }
    }
}