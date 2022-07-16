﻿using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq.Protected;
using Ptiw.Libs.Common;
using Ptiw.Libs.Validation.Validators;
using System.Net;

namespace Ptiw.Tests
{
    internal static class TestHelper
    {
        internal static void SetEnv()
        {
            Environment.SetEnvironmentVariable($"{Constants.SettingNames.Jobs}:GetAppointments:URL", $"https://reg.somethingtesturl.te");
            Environment.SetEnvironmentVariable($"{Constants.SettingNames.Jobs}:GetAppointments:Enabled", $"true");
            Environment.SetEnvironmentVariable($"{Constants.SettingNames.Jobs}:Notifier:Enabled", $"true");
            Environment.SetEnvironmentVariable($"{Constants.SettingNames.ServiceContext}", $"empty");
            Environment.SetEnvironmentVariable($"{Constants.SettingNames.TelegramToken}", $"0123456789:aaaaabbbbbcccccdddddeeeeefffffddddd");
            Environment.SetEnvironmentVariable($"{Constants.SettingNames.AdminTelegramToken}", $"0123456789:aaaaabbbbbcccccdddddeeeeefffffddddd");
            Environment.SetEnvironmentVariable($"{Constants.SettingNames.AdminUserId}", $"012345678");
        }

        internal static void ValidateEnv()
        {
            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var configValidator = new HostIConfigurationValidator();
            configValidator.ValidateAndThrow(config);
        }

        internal static HttpClient HttpClient
        {
            get
            {
                // ARRANGE
                var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                handlerMock
                   .Protected()
                   // Setup the PROTECTED method to mock
                   .Setup<Task<HttpResponseMessage>>(
                      "SendAsync",
                      ItExpr.IsAny<HttpRequestMessage>(),
                      ItExpr.IsAny<CancellationToken>()
                   )
                   // prepare the expected response of the mocked http call
                   .ReturnsAsync(new HttpResponseMessage()
                   {
                       StatusCode = HttpStatusCode.OK
                   })
                   .Verifiable();

                // use real http client with mocked handler here
                return new HttpClient(handlerMock.Object)
                {
                    BaseAddress = new Uri("http://test.com/"),
                };
            }
        }

        internal static ILogger Logger => NullLogger.Instance;
        internal static IConfiguration Configuration => new ConfigurationBuilder().AddEnvironmentVariables().Build();

        internal static Mock<ISchedulerFactory> MockSchedulerFactory
        {
            get
            {
                var factory = new Mock<ISchedulerFactory>();
                factory.Setup(f => f.GetScheduler(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Mock<IScheduler>().Object));
                return factory;
            }
        }

        internal static Mock<ServiceContext> MockServiceContext =>
            new Mock<ServiceContext>(new DbContextOptions<ServiceContext>(), Configuration);

        internal static async Task WaitUntil(Func<bool> predicate, int sleep = 50)
        {
            while (!predicate())
            {
                await Task.Delay(sleep);
            }
        }

        internal static void AssertEvents(this ReactionManager reactionManager, Type job)
        {
            Assert.NotNull(reactionManager.Errors);
            Assert.Empty(reactionManager.Errors);
            Assert.NotNull(reactionManager.OnCompletedReactions);
            Assert.NotEmpty(reactionManager.OnCompletedReactions);
            Assert.NotNull(reactionManager.OnNextReactions);
            Assert.NotEmpty(reactionManager.OnNextReactions);
            Assert.Contains(reactionManager.OnCompletedReactions, r => r.JobReactTo == null);
            Assert.Contains(reactionManager.OnNextReactions, r => r.JobReactTo == job);
            Assert.True(reactionManager.OnNextReactions.All(r => r.Task.IsCompletedSuccessfully));
            Assert.True(reactionManager.OnCompletedReactions.All(r => r.Task.IsCompletedSuccessfully));
        }
    }
}