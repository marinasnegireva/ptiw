using static Ptiw.Libs.Common.Enums;

namespace Ptiw.Libs.Common.Contracts
{
    public class FindAppointmentsConfig
    {
        public long UserId { get; set; }
        public int? HourNoEarlierThen { get; set; }
        public int? HourNoLaterThen { get; set; }
        public List<string>? SpecificDoctorList { get; set; }
        public DoctorListAction? DoctorListAction { get; set; } = Enums.DoctorListAction.seek;
        public List<DayOfWeek>? DaysOfWeek { get; set; }
    }
}