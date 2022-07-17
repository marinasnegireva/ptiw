namespace Ptiw.Tests
{
    public class EventSystemTests
    {
        //  private Mock<IObserver<JobCompletionData>> jobMonitor;
        private HttpClient httpClient;

        private IConfiguration config;
        private readonly Mock<ISchedulerFactory> mockSchedulerFactory;
        private readonly Mock<ServiceContext> serviceContext;

        public EventSystemTests()
        {
            TestHelper.SetEnv();
            TestHelper.ValidateEnv();

            httpClient = TestHelper.HttpClient;
            config = TestHelper.Configuration;
            serviceContext = TestHelper.MockServiceContext;
            mockSchedulerFactory = TestHelper.MockSchedulerFactory;
        }

        [Fact]
        public void BasicCalls_1()
        {
            var context = new Mock<ServiceContext>(new DbContextOptions<ServiceContext>(), config);
            var jobMonitor = new Mock<IObserver<JobCompletionData>>();
            var task = new Host.Jobs.Clinic.GetAppointments.Job(TestHelper.Logger, context.Object, httpClient, config, jobMonitor.Object,
                new Host.Jobs.Clinic.GetAppointments.Validator());
            task.Execute(new Mock<IJobExecutionContext>().Object).Wait();

            jobMonitor.Verify(jm => jm.OnCompleted(), Times.Once);
            jobMonitor.Verify(jm => jm.OnNext(It.IsAny<JobCompletionData>()), Times.Once);
            jobMonitor.VerifyNoOtherCalls();
        }
    }
}