namespace Ptiw.Host.Jobs.Clinic.FindAppointmentsForUser
{
    public class Validator : AbstractValidator<Job>
    {
        public Validator()
        {
            RuleFor(job => job.TaskConfiguration.UserId).NotNull().NotEmpty();

            RuleFor(job => job.TaskConfiguration.HourNoEarlierThen).GreaterThan(job => job.TaskConfiguration.HourNoLaterThen);

            RuleFor(job => job.TaskConfiguration.DoctorListAction).NotNull().When(job => job.TaskConfiguration.SpecificDoctorList != null && job.TaskConfiguration.SpecificDoctorList.Any());
        }
    }
}