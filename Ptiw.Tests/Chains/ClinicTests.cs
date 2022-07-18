using Moq.Protected;
using Newtonsoft.Json;
using Ptiw.Libs.Common.Contracts;
using Ptiw.Libs.EF.Tables;
using System.Linq.Expressions;
using System.Net;

namespace Ptiw.Tests
{
    public class ClinicTests
    {
        private readonly Mock<ISchedulerFactory> mockSchedulerFactory;
        private IConfiguration config;
        private List<NpcpnAppointment> contextAppointmentsDbSet;
        private List<Notification> contextNotificationsDbSet;

        public ClinicTests()
        {
            TestHelper.SetEnv();
            TestHelper.ValidateEnv();
            Host.Jobs.Clinic.FindAppointmentsForUser.Starter.StartedOnce = true;
            contextAppointmentsDbSet = new List<NpcpnAppointment>
                {
                    //пятница
                    new NpcpnAppointment {Id = 321, Active = true, Appointment = new DateTime(2022,5,6,14,0,0), DoctorName = "Тестов", DoctorId = "123321"},
                    //суббота
                    new NpcpnAppointment {Id = 322, Active = true, Appointment = new DateTime(2022,5,7,10,30,0), DoctorName = "Волкова", DoctorId = "789987"},
                    //вскр
                    new NpcpnAppointment {Id = 323, Active = true, Appointment = new DateTime(2022,5,8,9,30,0), DoctorName = "Аранина", DoctorId = "132312"},
                    new NpcpnAppointment {Id = 324, Active = false, Appointment = new DateTime(2022,5,8,10,0,0), DoctorName = "Камеров", DoctorId = "4231"}
                };

            contextNotificationsDbSet = new List<Notification>
            {
                new Notification{ Id = 1, Added = DateTime.Now.AddMinutes(-1), UserIdTo = 12345, NotificationText = "Some text"},
                new Notification{ Id = 2, Added = DateTime.Now.AddMinutes(-1), UserIdTo = 12344, NotificationText = "Some text"}
            };
            config = TestHelper.Configuration;
            mockSchedulerFactory = TestHelper.MockSchedulerFactory;
        }

        [Fact]
        public async void ClinicChain_1_link()
        {
            var context = TestHelper.MockServiceContext;
            var npcpnAppointments = new Mock<DbSet<NpcpnAppointment>>();
            context.Setup(r => r.NpcpnAppointments).Returns(npcpnAppointments.Object);
            var reactionManager = new ReactionManager(TestHelper.ReactionLogger, mockSchedulerFactory.Object, context.Object);

            var task = new Host.Jobs.Clinic.GetAppointments.Job(TestHelper.Logger, context.Object, HttpClient, config,
                reactionManager, new Host.Jobs.Clinic.GetAppointments.Validator());
            task.Execute(new Mock<IJobExecutionContext>().Object).Wait();

            Assert.True(task.ChangesWereMade);
            await reactionManager.WaitReactionsToComplete();
            reactionManager.AssertEvents(task.GetType());

            npcpnAppointments.Verify(c => c.AddRangeAsync(It.Is<List<NpcpnAppointment>>(list => list.Count == 3), It.IsAny<CancellationToken>()), Times.Once);
        }

        private async void ClinicChain_2_link(FindAppointmentsConfig userConfig, Expression<Func<Notification, bool>> match, int newLogs)
        {
            var context = TestHelper.MockServiceContext;
            ContextSetup_2_link(userConfig.UserId, context, out var notifications);
            var jobContext = new Mock<IJobExecutionContext>();
            jobContext.Setup(jk => jk.JobDetail.JobDataMap.Get(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(userConfig));
            var reactionManager = new ReactionManager(TestHelper.ReactionLogger, mockSchedulerFactory.Object, context.Object);
            var task = new Host.Jobs.Clinic.FindAppointmentsForUser.Job(TestHelper.Logger, context.Object, config,
                reactionManager, new Host.Jobs.Clinic.FindAppointmentsForUser.Validator());
            task.Execute(jobContext.Object).Wait();

            await reactionManager.WaitReactionsToComplete();
            Assert.True(reactionManager.Errors.IsEmpty, JsonConvert.SerializeObject(reactionManager.Errors));

            if (newLogs == 0)
            {
                Assert.False(task.ChangesWereMade, "something changes, though it shouldnt");
                notifications.Verify(n => n.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
                context.Verify(n => n.AddDataToNotificationLog(It.IsAny<List<int>>(), It.IsAny<long>(), It.IsAny<Type>(), It.IsAny<bool>()), Times.Never);
                context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
                reactionManager.AssertEventsNoChanges(task.GetType());
            }
            else
            {
                Assert.True(task.ChangesWereMade, "No changes happened!");
                notifications.Verify(c => c.AddAsync(
                    It.Is(match),
                    It.IsAny<CancellationToken>()), Times.Once);
                context.Verify(c => c.AddDataToNotificationLog(
                It.Is<List<int>>(list => list.Count == newLogs),
                userConfig.UserId,
                typeof(Host.Jobs.Clinic.FindAppointmentsForUser.Job),
                false),
                Times.Once);
                context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
                reactionManager.AssertEvents(task.GetType());
            }
        }

        private void ClinicChain_2_link_empty(FindAppointmentsConfig userConfig)
        {
            ClinicChain_2_link(
                userConfig: userConfig,
                null,
                newLogs: 0);
        }

        [Trait("User filter", "Config: anything")]
        [Fact]
        public void ClinicChain_2_link_filter_1()
        {
            var userId = new Random().NextInt64();
            var userConfig = new FindAppointmentsConfig { UserId = userId };

            ClinicChain_2_link(
                userConfig: userConfig,
                match: n => n.UserIdTo == userId
                && n.NotificationText.Contains("Врач: Тестов")
                && n.NotificationText.Contains("Врач: Волкова")
                && n.NotificationText.Contains("Врач: Аранина")
                && !n.NotificationText.Contains("Врач: Камеров"),
                newLogs: 3);
        }

        [Trait("User filter", "Config: later then 14")]
        [Fact]
        public void ClinicChain_2_link_filter_2()
        {
            var userId = new Random().NextInt64();

            var userConfig = new FindAppointmentsConfig { UserId = userId, HourNoEarlierThen = 14 };

            ClinicChain_2_link(
                userConfig: userConfig,
                match: n => n.UserIdTo == userId
                && n.NotificationText.Contains("Врач: Тестов")
                && !n.NotificationText.Contains("Врач: Волкова")
                && !n.NotificationText.Contains("Врач: Аранина")
                && !n.NotificationText.Contains("Врач: Камеров"),
                newLogs: 1);
        }

        [Trait("User filter", "Config: 10-12 hours")]
        [Fact]
        public void ClinicChain_2_link_filter_3()
        {
            var userId = new Random().NextInt64();

            var userConfig = new FindAppointmentsConfig { UserId = userId, HourNoEarlierThen = 10, HourNoLaterThen = 12 };

            ClinicChain_2_link(
                userConfig: userConfig,
                match: n => n.UserIdTo == userId
                && !n.NotificationText.Contains("Врач: Тестов")
                && n.NotificationText.Contains("Врач: Волкова")
                && !n.NotificationText.Contains("Врач: Аранина")
                && !n.NotificationText.Contains("Врач: Камеров"),
                newLogs: 1);
        }

        [Trait("User filter", "Config: anytime weekends")]
        [Fact]
        public void ClinicChain_2_link_filter_4()
        {
            var userId = new Random().NextInt64();

            var userConfig = new FindAppointmentsConfig { UserId = userId, DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Sunday, DayOfWeek.Saturday } };
            ClinicChain_2_link(
                userConfig: userConfig,
                match: n => n.UserIdTo == userId
                && !n.NotificationText.Contains("Врач: Тестов")
                && n.NotificationText.Contains("Врач: Волкова")
                && n.NotificationText.Contains("Врач: Аранина")
                && !n.NotificationText.Contains("Врач: Камеров"),
                newLogs: 2);
        }

        [Trait("User filter", "Config: before 10 on weekends")]
        [Fact]
        public void ClinicChain_2_link_filter_5()
        {
            var userId = new Random().NextInt64();

            var userConfig = new FindAppointmentsConfig
            {
                UserId = userId,
                DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Sunday, DayOfWeek.Saturday },
                HourNoLaterThen = 10
            };
            Console.WriteLine("First");
            ClinicChain_2_link(
                userConfig: userConfig,
                match: n => n.UserIdTo == userId
                && !n.NotificationText.Contains("Врач: Тестов")
                && !n.NotificationText.Contains("Врач: Волкова")
                && n.NotificationText.Contains("Врач: Аранина")
                && !n.NotificationText.Contains("Врач: Камеров"),
                newLogs: 1);
        }

        [Trait("User filter", "Config: only certain doctor")]
        [Fact]
        public void ClinicChain_2_link_filter_6()
        {
            var userId = new Random().NextInt64();

            var userConfig = new FindAppointmentsConfig { UserId = userId, SpecificDoctorList = new List<string> { "Волкова" } };

            ClinicChain_2_link(
                userConfig: userConfig,
                match: n => n.UserIdTo == userId
                && !n.NotificationText.Contains("Врач: Тестов")
                && n.NotificationText.Contains("Врач: Волкова")
                && !n.NotificationText.Contains("Врач: Аранина")
                && !n.NotificationText.Contains("Врач: Камеров"),
                newLogs: 1);
        }

        [Trait("User filter", "Config: ignore certain doctor")]
        [Fact]
        public void ClinicChain_2_link_filter_7()
        {
            var userId = 4212375;

            var userConfig = new FindAppointmentsConfig
            {
                UserId = userId,
                SpecificDoctorList = new List<string> { "Волкова" },
                DoctorListAction = Libs.Common.Enums.DoctorListAction.ignore
            };

            ClinicChain_2_link(
                userConfig: userConfig,
                match: n => n.UserIdTo == userId
                && n.NotificationText.Contains("Врач: Тестов")
                && !n.NotificationText.Contains("Врач: Волкова")
                && n.NotificationText.Contains("Врач: Аранина")
                && !n.NotificationText.Contains("Врач: Камеров"),
                newLogs: 2);
        }

        [Trait("Empty results", "Config: only certain doctor")]
        [Fact]
        public void ClinicChain_2_link_empty_1()
        {
            var userId = new Random().NextInt64();

            var userConfig = new FindAppointmentsConfig { UserId = userId, SpecificDoctorList = new List<string> { "Нет такого в списке" } };

            ClinicChain_2_link_empty(userConfig);
        }

        [Trait("Empty results", "Config: days of weeeks")]
        [Fact]
        public void ClinicChain_2_link_empty_2()
        {
            var userId = new Random().NextInt64();

            var userConfig = new FindAppointmentsConfig { UserId = userId, DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday } };

            ClinicChain_2_link_empty(userConfig);
        }

        [Trait("Empty results", "Config: too late")]
        [Fact]
        public void ClinicChain_2_link_empty_3()
        {
            var userId = 4212375;

            var userConfig = new FindAppointmentsConfig { UserId = userId, HourNoEarlierThen = 21 };

            ClinicChain_2_link_empty(userConfig);
        }

        [Trait("Empty results", "Config: too early")]
        [Fact]
        public void ClinicChain_2_link_empty_4()
        {
            var userId = 4212375;

            var userConfig = new FindAppointmentsConfig { UserId = userId, HourNoLaterThen = 6 };

            ClinicChain_2_link_empty(userConfig);
        }

        [Trait("Empty results", "Config: ignore everyone")]
        [Fact]
        public void ClinicChain_2_link_empty_5()
        {
            var userId = new Random().NextInt64();

            var userConfig = new FindAppointmentsConfig
            {
                UserId = userId,
                SpecificDoctorList = new List<string> { "Тестов", "Аранина", "Волкова", "Камеров" },
                DoctorListAction = Libs.Common.Enums.DoctorListAction.ignore
            };

            ClinicChain_2_link_empty(userConfig);
        }

        private void ContextSetup_2_link(long userId, Mock<ServiceContext> context, out Mock<DbSet<Notification>> notifications)
        {
            notifications = new Mock<DbSet<Notification>>();
            context.Setup(r => r.Notifications).Returns(notifications.Object);
            context
                .Setup(c => c.GetAppointmentsUserWasntNotifiedAbout(userId, typeof(Host.Jobs.Clinic.FindAppointmentsForUser.Job)))
                .Returns(contextAppointmentsDbSet.Where(c => c.Active == true).ToList());
        }

        [Fact]
        public async void ClinicChain_3_link()
        {
            var context = TestHelper.MockServiceContext;
            var notifications = new Mock<DbSet<Notification>>();
            context.Setup(r => r.Notifications).Returns(notifications.Object);
            context.Setup(r => r.GetNotificationsToSend()).Returns(contextNotificationsDbSet);

            var reactionManager = new ReactionManager(TestHelper.ReactionLogger, mockSchedulerFactory.Object, context.Object);

            var task = new Host.Jobs.Notifier.Job(TestHelper.Logger, context.Object, config,
                reactionManager, new Host.Jobs.Notifier.Validator());
            task.Execute(new Mock<IJobExecutionContext>().Object).Wait();

            Assert.True(task.ChangesWereMade);
            await reactionManager.WaitReactionsToComplete();

            notifications.Verify(c => c.UpdateRange(It.IsAny<IEnumerable<Notification>>()));
            Assert.NotNull(reactionManager.Errors);
            Assert.True(reactionManager.Errors.IsEmpty, JsonConvert.SerializeObject(reactionManager.Errors));
            Assert.NotNull(reactionManager.OnCompletedReactions);
            Assert.NotEmpty(reactionManager.OnCompletedReactions);
        }

        private static HttpClient HttpClient
        {
            get
            {
                // ARRANGE
                var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                handlerMock
                   .Protected()
                   // GetAppointableSeancesResponse
                   .Setup<Task<HttpResponseMessage>>(
                      "SendAsync",
                      ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get && m.RequestUri.AbsoluteUri.Contains("appointableSeances")),
                      ItExpr.IsAny<CancellationToken>()
                   )
                   // prepare the expected response of the mocked http call
                   .ReturnsAsync(new HttpResponseMessage()
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent("[{\"id\":\"2022080918401632217755992\",\"time\":\"18:40:00\"},{\"id\":\"2022080919001632217755992\",\"time\":\"19:00:00\"},{\"id\":\"2022080919401632217755992\",\"time\":\"19:40:00\"}]")
                   })
                   .Verifiable();

                handlerMock
                  .Protected()
                  // GetAppointableDoctorsResponse
                  .Setup<Task<HttpResponseMessage>>(
                     "SendAsync",
                     ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get && m.RequestUri.AbsoluteUri.Contains("appointableDoctors")),
                     ItExpr.IsAny<CancellationToken>()
                  )
                  // prepare the expected response of the mocked http call
                  .ReturnsAsync(new HttpResponseMessage()
                  {
                      StatusCode = HttpStatusCode.OK,
                      Content = new StringContent("[\r\n  {\r\n    \"id\": \"1632217755992\",\r\n    \"lastName\": \"Прохорович\",\r\n   " +
                      " \"firstName\": \"Валерия\",\r\n  " +
                      "  \"patronymic\": \"Сергеевна\",\r\n    " +
                      "\"info\": \"Врач\"\r\n  }]")
                  })
                  .Verifiable();

                handlerMock
                  .Protected()
                  // GetAppointableDatesResponse
                  .Setup<Task<HttpResponseMessage>>(
                     "SendAsync",
                     ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get && m.RequestUri.AbsoluteUri.Contains("appointableDates")),
                     ItExpr.IsAny<CancellationToken>()
                  )
                  // prepare the expected response of the mocked http call
                  .ReturnsAsync(new HttpResponseMessage()
                  {
                      StatusCode = HttpStatusCode.OK,
                      Content = new StringContent("[ \"2022-08-08\" ]")
                  })
                  .Verifiable();

                // use real http client with mocked handler here
                return new HttpClient(handlerMock.Object);
            }
        }
    }
}