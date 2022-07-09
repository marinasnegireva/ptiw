using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Ptiw.DataAccess;
using Ptiw.HostApp.Tasks.CheckNpcpnSchedule;
using Quartz;

namespace Ptiw.Tests
{
    public class TaskCreationTests
    {
        public TaskCreationTests()
        {
            Environment.SetEnvironmentVariable($"Tasks:{nameof(FindAppointmentScheduleTask)}:URL", $"https://reg.{Guid.NewGuid()}.testurl.test");
            Environment.SetEnvironmentVariable($"ServiceContext", $"empty");
        }

        [Fact]
        public void TaskCreationTest1()
        {
            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var logger = new Mock<ILogger<FindAppointmentScheduleTask>>();
            var context = new Mock<ServiceContext>(config);
            var httpclient = new Mock<HttpClient>();

            var task = new FindAppointmentScheduleTask(logger.Object, context.Object, httpclient.Object, config);
            task.Execute(new Mock<IJobExecutionContext>().Object).Wait();
        }
    }
}