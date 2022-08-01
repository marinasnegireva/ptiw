using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Ptiw.Libs.Common.Contracts;
using Ptiw.Libs.EF.Tables;

namespace Ptiw.Libs.EF
{
    public class ServiceContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public ServiceContext(DbContextOptions<ServiceContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration[Constants.SettingNames.ServiceContext], b => b.MigrationsAssembly("Ptiw.Host"));
        }

        public void AddDefaultTaskConfig()
        {
            if (TaskConfigs.Count() == 0)
            {
                TaskConfigs.Add(new TaskConfig
                {
                    Added = DateTime.UtcNow,
                    Enabled = true,
                    JobTypeFullName = "Ptiw.Host.Jobs.Clinic.FindAppointmentsForUser.Job",
                    BelongsToUser = long.Parse(_configuration[Constants.SettingNames.AdminUserId]),
                    Config = JsonConvert.SerializeObject(new FindAppointmentsConfig
                    {
                        UserId = long.Parse(_configuration[Constants.SettingNames.AdminUserId])
                    })
                });
                SaveChanges();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");
        }


        public virtual DbSet<NpcpnAppointment> NpcpnAppointments { get; set; }
        public virtual DbSet<UserNotificationsAggregateLogEntry> UserNotificationsAggregateLog { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<TaskConfig> TaskConfigs { get; set; }
        public virtual DbSet<User> Users { get; set; }

        public virtual bool AlreadyExists(NpcpnAppointment data)
        {
            return NpcpnAppointments.Any(n => n == data);
        }

        public virtual List<NpcpnAppointment> GetAppointmentsUserWasntNotifiedAbout(long userId, Type jobtype)
        {
            return (List<NpcpnAppointment>)(from na in NpcpnAppointments
                                            join notification in UserNotificationsAggregateLog on na.Id equals notification.FkId
                                            where notification.UserId != userId && notification.JobTypeFullName == jobtype.FullName && na.Active
                                            select na);
        }

        public virtual List<Notification> GetNotificationsToSend()
        {
            return Notifications.Where(t => !t.Completed).ToList();
        }

        public virtual async Task AddDataToNotificationLog(List<int> ids, long userId, Type jobType, bool saveContext = true)
        {
            var newNotificationLogData = ids.Select(id => new UserNotificationsAggregateLogEntry
            {
                FkId = id,
                UserId = userId,
                JobTypeFullName = jobType.FullName
            });
            await UserNotificationsAggregateLog.AddRangeAsync(newNotificationLogData);
            if (saveContext)
            {
                await SaveChangesAsync();
            }
        }

        private async void ProcessUpdates(object sender, EntityEntryEventArgs e)
        {
            if (e.Entry.Entity is NpcpnAppointment appointment)
            {
                switch (e.Entry.State)
                {
                    case EntityState.Added:

                        break;
                }
            }
        }
    }
}