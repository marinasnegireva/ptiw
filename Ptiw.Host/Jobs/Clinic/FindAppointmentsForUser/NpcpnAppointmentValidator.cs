using Ptiw.Libs.EF.Tables;
using Ptiw.Libs.Validation;
using static Ptiw.Libs.Common.Enums;

namespace Ptiw.Host.Jobs.Clinic.FindAppointmentsForUser
{
#pragma warning disable CS8629 // Nullable value type may be null. : Because it may not
#pragma warning disable CS8602 // Dereference of a possibly null reference.

    public class NpcpnAppointmentValidator : AbstractValidator<NpcpnAppointment>
    {
        public NpcpnAppointmentValidator()
        {
            RuleFor(result => result.TaskConfigForFiltering).NotEmpty().WithMessage("Config should be added for validation");

            RuleFor(result => result.Appointment).NotNull();

            RuleFor(result => result.Appointment.TimeOfDay).GreaterThanOrEqualTo(result => new TimeSpan(result.TaskConfigForFiltering.HourNoEarlierThen.Value, 0, 0))
                .When(result => result.TaskConfigForFiltering.HourNoEarlierThen.HasValue);

            RuleFor(result => result.Appointment.TimeOfDay).LessThanOrEqualTo(result => new TimeSpan(result.TaskConfigForFiltering.HourNoLaterThen.Value, 0, 0))
                .When(result => result.TaskConfigForFiltering.HourNoLaterThen.HasValue);

            When(result => result.TaskConfigForFiltering.SpecificDoctorList != null && result.TaskConfigForFiltering.SpecificDoctorList.Any(), () =>
            {
                RuleFor(result => result.DoctorName).NotNull().NotEmpty()
                .Must((result, DoctorName) => DoctorName.ContainsAnyOf(result.TaskConfigForFiltering.SpecificDoctorList ?? new List<string>()))
                .When(result => result.TaskConfigForFiltering.DoctorListAction == DoctorListAction.seek);

                RuleFor(result => result.DoctorName).NotNull().NotEmpty()
                .Must((result, DoctorName) => !DoctorName.ContainsAnyOf(result.TaskConfigForFiltering.SpecificDoctorList ?? new List<string>()))
                .When(result => result.TaskConfigForFiltering.DoctorListAction == DoctorListAction.ignore);
            });

            When(result => result.TaskConfigForFiltering.DaysOfWeek != null && result.TaskConfigForFiltering.DaysOfWeek.Any(), () =>
            {
                RuleFor(result => result.Appointment.DayOfWeek)
                .Must((result, AppointmentDayOfWeek) => result.TaskConfigForFiltering.DaysOfWeek.Contains(AppointmentDayOfWeek));
            });
        }
    }

#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
}