using Ptiw.HostApp.Tasks.CheckNpcpnSchedule.ResponseModels;
using Ptiw.Jobs.QuartzJobs;
using Ptiw.Libs.EF.Tables;
using static Ptiw.Libs.Common.Constants;

namespace Ptiw.Host.Jobs.Clinic.GetAppointments
{
    [DisallowConcurrentExecution]
    public class Job : AbstractJob
    {
        private readonly ServiceContext _serviceContext;
        private readonly HttpClient _httpClient;
        private List<NpcpnAppointment> _finalResult;

        public Uri? ClinicUri => new(GetTaskData(SettingNames.URL));

        public Job(ILogger logger, ServiceContext serviceContext, HttpClient httpClient,
            IConfiguration configuration, IObserver<JobCompletionData> jobMonitor, IValidator<Job> validator)
            : base(logger, configuration, jobMonitor)
        {
            validator.ValidateAndThrow(this);
            _serviceContext = serviceContext;
            _httpClient = httpClient;
            _finalResult = new List<NpcpnAppointment>();
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Logger.LogDebug("Started!");

                if (IsEnabled == false)
                {
                    await context.Scheduler.PauseJob(context.JobDetail.Key);
                    return;
                }

                var dates = await GetAppointableDates();
                await GetAppointableDoctors(dates);
                await GetAppointableSeances();
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
                Logger.LogInformation($"Aggregation of {_finalResult.Count} appointments in schedule right now.");
                var newAppointments = _finalResult
                .Where(fr => !_serviceContext.AlreadyExists(fr))
                .ToList();
                if (newAppointments.Any())
                {
                    await _serviceContext.NpcpnAppointments.AddRangeAsync(newAppointments);
                    Logger.LogInformation($"Saving {newAppointments.Count} new appointments.");
                }
                var stillActiveAppointments = _finalResult
                    .Where(fr => _serviceContext.AlreadyExists(fr))
                    .ToList();

                if (stillActiveAppointments.Any())
                {
                    var inactiveAppointments = _serviceContext.NpcpnAppointments.Where(na => na.Active && !stillActiveAppointments.Contains(na)).ToList();
                    inactiveAppointments.ForEach(na => na.Disable());
                    Logger.LogInformation($"Disabling {inactiveAppointments.Count} appointments.");
                }
                await _serviceContext.SaveChangesAsync();
                Logger.LogInformation("Saved.");
                ChangesWereMade = true;
            }
            else
            {
                Logger.LogInformation("No appointments found in api");
            }
        }

        private async Task<List<DateTime>> GetAppointableDates()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(ClinicUri + "appointableDates?affilateId=2")
            };

            var result = await _httpClient.SendAsync(request);
            if (!result.IsSuccessStatusCode)
            {
                Logger.LogError("GetAppointableDates ERROR: " + request.RequestUri + result.StatusCode + result.ReasonPhrase);
            }
            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<DateTime>>(content) ?? new List<DateTime>();
        }

        private async Task GetAppointableDoctors(List<DateTime> dateTimes)
        {
            foreach (var date in dateTimes)
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(ClinicUri + "appointableDoctors?affilateId=2&date=" + date.ToString("yyyy/MM/dd"))
                };

                var result = await _httpClient.SendAsync(request);
                if (!result.IsSuccessStatusCode)
                {
                    Logger.LogError("GetAppointableDoctors ERROR: " + request.RequestUri + result.StatusCode + result.ReasonPhrase);
                }
                var content = await result.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<AppointableDoctor>>(content) ?? new List<AppointableDoctor>();

                foreach (var appointableDoctor in list)
                {
                    _finalResult.Add(new NpcpnAppointment
                    {
                        AppointmentDate = date.ToString("dd.MM"),
                        AppointmentDayOfWeek = RuCulture.DateTimeFormat.GetDayName(date.DayOfWeek),
                        DoctorId = appointableDoctor.Id,
                        DoctorName = appointableDoctor.GetFullName(),
                    });
                }
            }
        }

        private async Task GetAppointableSeances()
        {
            var groupedResult = _finalResult.Where(fr => !string.IsNullOrEmpty(fr.DoctorId))
                .Select(fr => new Tuple<NpcpnAppointment, Task<List<AppointableSeance>>>(fr, GetSeancesAsync(fr)))
                .AsEnumerable()
                .ToList();
            _finalResult.Clear();

            foreach (var tuple in groupedResult)
            {
                var result = await tuple.Item2;
                var listToAdd = result.Select(a => new NpcpnAppointment()
                {
                    DoctorId = tuple.Item1.DoctorId,
                    DoctorName = tuple.Item1.DoctorName,
                    AppointmentTime = a.Time.ToString("HH.mm"),
                    AppointmentDate = tuple.Item1.AppointmentDate,
                    AppointmentDayOfWeek = tuple.Item1.AppointmentDayOfWeek,
                    Added = DateTime.UtcNow
                }).ToList();
                _finalResult.AddRange(listToAdd);
            }
        }

        private async Task<List<AppointableSeance>> GetSeancesAsync(NpcpnAppointment data)
        {
            var split = data.AppointmentDate.Split(".");
            var stringDate = $"{DateTime.Now.Year}/{split[1]}/{split[0]}";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(ClinicUri + "appointableSeances?affilateId=2&date=" + stringDate + "&doctorId=" + data.DoctorId)
            };

            var result = await _httpClient.SendAsync(request);
            if (!result.IsSuccessStatusCode)
            {
                Logger.LogError("GetAppointableSeances ERROR: " + request.RequestUri + result.StatusCode + result.ReasonPhrase);
            }
            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<AppointableSeance>>(content) ?? new List<AppointableSeance>();
        }
    }
}