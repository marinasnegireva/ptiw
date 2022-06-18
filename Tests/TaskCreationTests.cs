using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Ptiw.DataAccess;
using Ptiw.HostApp.Tasks.CheckNpcpnSchedule;
using Quartz;

namespace Ptiw.Tests
{
    public class TaskCreationTests
    {
        [Fact]
        public void TaskCreationTest1()
        {
            var logger = new Mock<ILogger<FindAppointmentScheduleTask>>();
            var context = new Mock<ServiceContext>(new DbContextOptions<ServiceContext>());
            var httpclient = new Mock<HttpClient>();
            var task = new FindAppointmentScheduleTask(logger.Object, context.Object, httpclient.Object);
            task.Execute(new Mock<IJobExecutionContext>().Object).Wait();
        }
    }
}