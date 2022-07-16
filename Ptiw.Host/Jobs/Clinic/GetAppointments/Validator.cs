namespace Ptiw.Host.Jobs.Clinic.GetAppointments
{
    public class Validator : AbstractValidator<Job>
    {
        public Validator()
        {
            RuleFor(job => job.IsEnabled).NotNull();
            RuleFor(job => job.ClinicUri).NotNull().Must(uri => !string.IsNullOrEmpty(uri.AbsoluteUri));
        }
    }
}