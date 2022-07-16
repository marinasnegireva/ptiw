namespace Ptiw.Tests
{
    public class JobTests
    {
        private Mock<IObserver<JobCompletionData>> jobMonitor;
        private HttpClient httpClient;
        private IConfiguration config;

        public JobTests()
        {
            TestHelper.SetEnv();
            TestHelper.ValidateEnv();

            config = TestHelper.Configuration;
            httpClient = TestHelper.HttpClient;

            jobMonitor = new Mock<IObserver<JobCompletionData>>();
        }

        [Fact]
        public void NotifierJobCreationTest()
        {
            var context = TestHelper.MockServiceContext;

            var task = new Host.Jobs.Notifier.Job(TestHelper.Logger, context.Object, config, jobMonitor.Object,
                new Host.Jobs.Notifier.Validator());
            task.Execute(new Mock<IJobExecutionContext>().Object).Wait();
        }

        [Fact]
        public void GetAppointmentsJobCreationTest()
        {
            var context = TestHelper.MockServiceContext;

            var task = new Host.Jobs.Clinic.GetAppointments.Job(TestHelper.Logger, context.Object, httpClient, config,
                jobMonitor.Object, new Host.Jobs.Clinic.GetAppointments.Validator());
            task.Execute(new Mock<IJobExecutionContext>().Object).Wait();
        }

        //[Fact]
        //public void SearchAppointmentsForUserJobCreationTest()
        //{
        //   // var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        //    var logger = new Mock<ILogger<SearchAppointmentsForUserJob>>();
        //    var context = new Mock<ServiceContext>(new DbContextOptions<ServiceContext>(), config);

        //    var task = new SearchAppointmentsForUserJob(logger.Object, context.Object, config, jobMonitor.Object, new SearchAppointmentsForUserJobConfigValidator()) ;
        //    task.Execute(new Mock<IJobExecutionContext>().Object).Wait();
        //}
    }
}