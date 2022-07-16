namespace Ptiw.Jobs.QuartzJobs
{
    public interface IExpendedJob : IJob
    {
        ILogger<IJob> Logger { get; set; }
        IConfiguration Configuration { get; set; }
    }
}