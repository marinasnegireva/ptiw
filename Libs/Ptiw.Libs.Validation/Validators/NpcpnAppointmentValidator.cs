using Ptiw.Libs.EF.Tables;
using static Ptiw.Libs.Common.Enums;

namespace Ptiw.Libs.Validation.Validators
{
#pragma warning disable CS8629 // Nullable value type may be null. : Because it may not
#pragma warning disable CS8602 // Dereference of a possibly null reference.

    public class NpcpnAppointmentValidator : AbstractValidator<NpcpnAppointment>
    {
        public NpcpnAppointmentValidator()
        {
            RuleFor(result => result.TaskConfigForFiltering).NotEmpty().WithMessage("Config should be added for validation");

            RuleFor(result => result.AppointmentTimeHour).NotNull();

            RuleFor(result => result.AppointmentTimeHour).GreaterThanOrEqualTo(result => result.TaskConfigForFiltering.HourNoEarlierThen.Value)
                .When(result => result.TaskConfigForFiltering.HourNoEarlierThen.HasValue);

            RuleFor(result => result.AppointmentTimeHour).LessThanOrEqualTo(result => result.TaskConfigForFiltering.HourNoLaterThen.Value)
                .When(result => result.TaskConfigForFiltering.HourNoLaterThen.HasValue);

            When(result => result.TaskConfigForFiltering.SpecificDoctorList != null && result.TaskConfigForFiltering.SpecificDoctorList.Any(), () =>
            {
                RuleFor(result => result.DoctorName).NotNull().NotEmpty().Must(DoctorName => DoctorName.ContainsAnyOf(new List<string>()))
                .When(result => result.TaskConfigForFiltering.DoctorListAction == DoctorListAction.seek);
                RuleFor(result => result.DoctorName).NotNull().NotEmpty().Must(DoctorName => !DoctorName.ContainsAnyOf(new List<string>()))
                .When(result => result.TaskConfigForFiltering.DoctorListAction == DoctorListAction.ignore);
            });

            When(result => result.TaskConfigForFiltering.DaysOfWeek != null && result.TaskConfigForFiltering.DaysOfWeek.Any(), () =>
            {
                RuleFor(result => result.AppointmentDayOfWeek).NotNull().NotEmpty()
                .Must((result, AppointmentDayOfWeek) => result.TaskConfigForFiltering.DayOfWeekNames.Contains(AppointmentDayOfWeek));
            });
        }
    }

#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
}