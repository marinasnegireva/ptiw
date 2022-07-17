namespace Ptiw.Jobs.QuartzJobs
{
    public interface IExpendedJob : IJob
    {
        ILogger<IExpendedJob> Logger { get; set; }
        IConfiguration Configuration { get; set; }
    }
}