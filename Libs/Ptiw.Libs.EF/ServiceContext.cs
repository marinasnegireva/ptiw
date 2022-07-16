using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Configure default schema
            modelBuilder.HasDefaultSchema("public");
        }

        public DbSet<NpcpnAppointment> NpcpnAppointments { get; set; }
        public DbSet<UserNotificationsAggregateLogEntry> UserNotificationsAggregateLog { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TaskConfig> TaskConfigs { get; set; }
        public DbSet<User> Users { get; set; }

        public bool AlreadyExists(NpcpnAppointment data)
        {
            return NpcpnAppointments.Any(n => n == data);
        }

        public List<NpcpnAppointment> GetAppoitmentsUserWasntNotifiedAbout(long userId, string JobName)
        {
            return (List<NpcpnAppointment>)(from na in NpcpnAppointments
                                            join notification in UserNotificationsAggregateLog on na.Id equals notification.FkId
                                            where notification.UserId != userId && notification.JobName == JobName
                                            select na);
        }

        public async Task AddDataToNotificationLog(List<int> ids, long userId, string JobName, bool saveContext = true)
        {
            var newNotificationLogData = ids.Select(id => new UserNotificationsAggregateLogEntry { FkId = id, UserId = userId, JobName = JobName });
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