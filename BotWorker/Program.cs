using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Hosting;
using Quartz;
using Telegram.Bot;

namespace Ptiw.HostApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<ServiceContext>(options =>
                        options.UseNpgsql(Settings.SettingsManager.AppSettings.ServiceContext));
                    services.AddHttpClient();
                    services.AddSingleton<TelegramBotClient>(
                       x => ActivatorUtilities.CreateInstance<TelegramBotClient>(x, Settings.SettingsManager.AppSettings.TelegramToken));

                    services.AddQuartz(q =>
                     {
                         q.UseMicrosoftDependencyInjectionJobFactory();
                         q.UseSimpleTypeLoader();
                         q.UseInMemoryStore();
                         q.UseDefaultThreadPool(tp =>
                         {
                             tp.MaxConcurrency = 10;
                         });

                         q.ScheduleJob<FindAppointmentScheduleTask>(trigger => trigger
                         .WithIdentity("CheckNpcpnSchedule")
                         .WithSimpleSchedule(x => x.WithIntervalInMinutes(4).RepeatForever())
                         .StartNow()
                         .WithDescription("Looking for appointments")
                         );

                         q.ScheduleJob<NpcpnNotifierTask>(trigger => trigger
                         .WithIdentity("NpcpnNotifierTask")
                         .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever())
                         .StartNow()
                         .WithDescription("Notifies about appointments")
                         );
                     });

                    // ASP.NET Core hosting
                    services.AddQuartzServer(options =>
                    {
                        // when shutting down we want jobs to complete gracefully
                        options.WaitForJobsToComplete = true;
                    });

                    // services.AddHostedService<BotWorker>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
               .UseNLog()
            ;
    }
}