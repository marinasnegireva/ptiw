namespace Ptiw.Host.Jobs.Clinic.FindAppointmentsForUser
{
    public class Validator : AbstractValidator<Job>
    {
        public Validator()
        {
            RuleFor(job => job.TaskConfiguration.UserId).NotNull().NotEmpty();

            RuleFor(job => job.TaskConfiguration.HourNoEarlierThen).LessThan(job => job.TaskConfiguration.HourNoLaterThen)
            .When(job => job.TaskConfiguration.HourNoEarlierThen != null && job.TaskConfiguration.HourNoLaterThen != null);

            RuleFor(job => job.TaskConfiguration.DoctorListAction).NotNull()
                .When(job => job.TaskConfiguration.SpecificDoctorList != null && job.TaskConfiguration.SpecificDoctorList.Any());
        }
    }
}