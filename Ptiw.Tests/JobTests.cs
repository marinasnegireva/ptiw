using Newtonsoft.Json;
using Ptiw.Libs.Common.Contracts;

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

        [Fact]
        public void SearchAppointmentsForUserJobCreationTest()
        {
            var context = TestHelper.MockServiceContext;
            context
                .Setup(c => c.GetAppointmentsUserWasntNotifiedAbout(0, typeof(Host.Jobs.Clinic.FindAppointmentsForUser.Job)))
                .Returns(new List<Libs.EF.Tables.NpcpnAppointment>
                {
                    new Libs.EF.Tables.NpcpnAppointment { Active = true, Appointment = DateTime.Now.AddDays(1) }
                });

            var userConfig = new FindAppointmentsConfig { UserId = 123 };
            var jobContext = new Mock<IJobExecutionContext>();
            jobContext.Setup(jk => jk.JobDetail.JobDataMap.Get(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(userConfig));

            var task = new Host.Jobs.Clinic.FindAppointmentsForUser.Job(TestHelper.Logger, context.Object, config,
                jobMonitor.Object, new Host.Jobs.Clinic.FindAppointmentsForUser.Validator());
            task.Execute(jobContext.Object).Wait();
        }
    }
}