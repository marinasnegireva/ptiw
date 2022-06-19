using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ptiw.DataAccess.Tables;
using System.Linq;

namespace Ptiw.DataAccess
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
            optionsBuilder.UseNpgsql(_configuration["ServiceContext"]);
        }

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