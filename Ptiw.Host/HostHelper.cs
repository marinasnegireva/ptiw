using Ptiw.Libs.Telegram.Logging;
using static Ptiw.Libs.Common.Constants;

namespace Ptiw.Host
{
    public static class HostHelper
    {
        public static IHostBuilder GetBuilder()
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<ServiceContext>();
                services.AddTransient<HttpClient>();
                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();
                    q.UseSimpleTypeLoader();
                    q.UseInMemoryStore();
                    q.UseDefaultThreadPool(tp =>
                    {
                        tp.MaxConcurrency = 10;
                    });

                    q.ScheduleJob<Jobs.Clinic.GetAppointments.Job>(trigger => trigger
                    .WithIdentity("GetAppointmentsJob")
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(4).RepeatForever())
                    .StartNow()
                    .WithDescription("Looking for appointments")
                    );

                    /*q.ScheduleJob<NotifierJob>(trigger => trigger
                    .WithIdentity("UniversalNotifierTask")
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever())
                    .StartNow()
                    .WithDescription("Universal notifier")
                    );*/
                });

                services.AddQuartzHostedService(options =>
                {
                    // when shutting down we want jobs to complete gracefully
                    options.WaitForJobsToComplete = true;
                });

                //Telegram bot worker
                //services.AddHostedService<BotWorker>();

                services.AddTransient<IObserver<JobCompletionData>, ReactionManager>();

                //Validators
                services.AddTransient<IValidator<Jobs.Clinic.FindAppointmentsForUser.Job>, Jobs.Clinic.FindAppointmentsForUser.Validator>();

                services.AddTransient<IValidator<Jobs.Clinic.GetAppointments.Job>, Jobs.Clinic.GetAppointments.Validator>();

                services.AddTransient<IValidator<Jobs.Notifier.Job>, Jobs.Notifier.Validator>();
            })
            .ConfigureLogging((context, collection) =>
            {
                collection.ClearProviders();
                collection.SetMinimumLevel(LogLevel.Trace);
                collection.AddConsole();

                if (context.Configuration != null)
                {
                    collection.AddTelegram(new TelegramLoggerOptions(LogLevel.Information)
                    {
                        AccessToken = context.Configuration[SettingNames.AdminTelegramToken],
                        ChatId = context.Configuration[SettingNames.AdminUserId],
                        UseEmoji = false
                    });
                }
            });
        }

        public static void ValidateConfigs()
        {
            var configBuilder = new ConfigurationBuilder().AddEnvironmentVariables();
            configBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            var config = configBuilder.Build();

            var configValidator = new Libs.Validation.Validators.HostIConfigurationValidator();
            configValidator.ValidateAndThrow(config);
        }
    }
}