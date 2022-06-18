using Microsoft.EntityFrameworkCore;
using Ptiw.DataAccess.Tables;
using System.Linq;

namespace Ptiw.DataAccess
{
    public class ServiceContext : DbContext
    {
        public ServiceContext(DbContextOptions<ServiceContext> options) : base(options)
        {
        }

        /*  private readonly string connectionString;
          public ServiceContext(string connectionString)
          {
              this.connectionString = connectionString;
          }

          protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
          {
              optionsBuilder.UseNpgsql(connectionString);
          }
        */

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Configure default schema
            modelBuilder.HasDefaultSchema("public");
        }

        public DbSet<FindAppointmentTaskData> FindAppointmentTaskLog { get; set; }

        public bool TaskAlreadyExists(FindAppointmentTaskData data)
        {
            return FindAppointmentTaskLog.Any(n => n.DoctorName.Trim() == data.DoctorName.Trim() &&
                   n.DoctorId.Trim() == data.DoctorId.Trim() &&
                   n.AppointmentTime.Trim() == data.AppointmentTime.Trim() &&
                   n.AppointmentDate.Trim() == data.AppointmentDate.Trim());
        }
    }
}