using Microsoft.Extensions.DependencyInjection;
using NLog.Extensions.Hosting;
using System.IO;

var builder = Host.CreateDefaultBuilder(args);

var host = builder
    .ConfigureHostConfiguration(hostConfig =>
    {
        hostConfig.SetBasePath(Directory.GetCurrentDirectory());
#if DEBUG
        hostConfig.AddJsonFile("appsettings.Development.json", optional: false);
#endif
    })
    .ConfigureServices(services =>
    {
        services.AddDbContext<ServiceContext>();
        services.AddHttpClient();
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
    })
    .ConfigureLogging(logging =>
      {
          logging.ClearProviders();
          logging.SetMinimumLevel(LogLevel.Trace);
      })
    .UseNLog()
    .Build();

await host.RunAsync();