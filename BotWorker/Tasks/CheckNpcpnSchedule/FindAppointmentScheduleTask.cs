using Ptiw.DataAccess.Tables;
using Ptiw.HostApp.Tasks.CheckNpcpnSchedule.Responses;
using Quartz;
using System.Globalization;
using System.Net.Http;

namespace Ptiw.HostApp.Tasks.CheckNpcpnSchedule
{
    [DisallowConcurrentExecution]
    public class FindAppointmentScheduleTask : IJob
    {
        private readonly ILogger<FindAppointmentScheduleTask> _logger;
        private readonly ServiceContext _serviceContext;
        private readonly HttpClient _httpClient;
        private readonly Uri _baseUri;
        private List<FindAppointmentTaskData> _finalResult;
        private readonly CultureInfo culture = new CultureInfo("ru-RU");
        private readonly IConfiguration _configuration;

        public FindAppointmentScheduleTask(ILogger<FindAppointmentScheduleTask> logger, ServiceContext serviceContext, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceContext = serviceContext;
            _httpClient = httpClient;
            _baseUri = new(this.GetTaskData(_configuration, "URL"));
            _finalResult = new List<FindAppointmentTaskData>();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogDebug("Started!");
                _serviceContext.FindAppointmentTaskLog.ToArray();
                var dates = await GetAppointableDates();
                await GetAppointableDoctors(dates);
                GetAppointableSeances();
                FilterSeances();
                await _serviceContext.FindAppointmentTaskLog.AddRangeAsync(_finalResult);
                await _serviceContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }

        private async Task<List<DateTime>> GetAppointableDates()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_baseUri + "appointableDates?affilateId=2")
            };

            var result = await _httpClient.SendAsync(request);
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError("GetAppointableDates ERROR: " + request.RequestUri + result.StatusCode + result.ReasonPhrase);
            }
            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<DateTime>>(content);
        }

        private async Task GetAppointableDoctors(List<DateTime> dateTimes)
        {
            foreach (var date in dateTimes)
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(_baseUri + "appointableDoctors?affilateId=2&date=" + date.ToString("yyyy/MM/dd"))
                };

                var result = await _httpClient.SendAsync(request);
                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError("GetAppointableDoctors ERROR: " + request.RequestUri + result.StatusCode + result.ReasonPhrase);
                }
                var content = await result.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<AppointableDoctor>>(content);
                if (!list.IsNullOrEmpty())
                {
                    foreach (var appointableDoctor in list)
                    {
                        _finalResult.Add(new FindAppointmentTaskData
                        {
                            AppointmentDate = date.ToString("dd.MM"),
                            AppointmentDayOfWeek = culture.DateTimeFormat.GetDayName(date.DayOfWeek),
                            DoctorId = appointableDoctor.Id,
                            DoctorName = appointableDoctor.GetFullName(),
                        }); ;
                    }
                }
            }
        }

        private void GetAppointableSeances()
        {
            var groupedResult = _finalResult.Where(fr => !string.IsNullOrEmpty(fr.DoctorId))
                .Select(fr => new Tuple<FindAppointmentTaskData, List<AppointableSeance>>(fr, GetSeancesAsync(fr).Result))
                .AsEnumerable()
                .ToList();
            _finalResult.Clear();

            foreach (var tuple in groupedResult)
            {
                var listToAdd = tuple.Item2.Select(a => new FindAppointmentTaskData()
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

        private async Task<List<AppointableSeance>> GetSeancesAsync(FindAppointmentTaskData data)
        {
            var split = data.AppointmentDate.Split(".");
            var stringDate = $"{DateTime.Now.Year}/{split[1]}/{split[0]}";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_baseUri + "appointableSeances?affilateId=2&date=" + stringDate + "&doctorId=" + data.DoctorId)
            };

            var result = await _httpClient.SendAsync(request);
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError("GetAppointableSeances ERROR: " + request.RequestUri + result.StatusCode + result.ReasonPhrase);
            }
            var content = await result.Content.ReadAsStringAsync();
            var seances = JsonConvert.DeserializeObject<List<AppointableSeance>>(content);
            return seances;
        }

        private void FilterSeances()
        {
            _finalResult = _finalResult
                //  .Where(fr => (fr.AppointmentDayOfWeek == culture.DateTimeFormat.GetDayName(DayOfWeek.Saturday) || Convert.ToInt32(fr.AppointmentTime.Split(".")[0]) == 19) && !fr.DoctorName.ToLower().Contains("семенкова евгения"))
                .Where(fr => Convert.ToInt32(fr.AppointmentTime.Split(".")[0]) > 13 && fr.DoctorName.ToLower().Contains("булгакова"))
                .Where(fr => !_serviceContext.TaskAlreadyExists(fr))
                .ToList();
        }
    }
}