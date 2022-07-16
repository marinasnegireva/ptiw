namespace Ptiw.Host.Jobs.Notifier
{
    public class Validator : AbstractValidator<Job>
    {
        public Validator()
        {
            RuleFor(job => job.IsEnabled).NotNull();
        }
    }
}