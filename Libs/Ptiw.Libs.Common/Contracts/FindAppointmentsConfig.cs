using System.Text.Json.Serialization;
using static Ptiw.Libs.Common.Enums;

namespace Ptiw.Libs.Common.Contracts
{
    public class SearchAppointmentsForUserConfig
    {
        public long UserId { get; set; }
        public int? HourNoEarlierThen { get; set; }
        public int? HourNoLaterThen { get; set; }
        public List<string>? SpecificDoctorList { get; set; }
        public DoctorListAction? DoctorListAction { get; set; }
        public List<DayOfWeek>? DaysOfWeek { get; set; }

        [JsonIgnore]
        public List<string>? DayOfWeekNames
        { get { return DaysOfWeek?.Select(d => Constants.RuCulture.DateTimeFormat.GetDayName(d))?.ToList(); } }
    }
}