namespace Ptiw.Jobs.QuartzJobs
{
    public interface IExpendedJob : IJob
    {
        ILogger Logger { get; set; }
        IConfiguration Configuration { get; set; }
    }
}