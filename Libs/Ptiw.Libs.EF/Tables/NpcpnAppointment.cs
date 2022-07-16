using Ptiw.Libs.Common.Contracts;

namespace Ptiw.Libs.EF.Tables
{
    /// <summary>
    /// Class for result data
    /// </summary>
    public class NpcpnAppointment : IEquatable<NpcpnAppointment>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(126)]
        public string DoctorName { get; set; }

        [Required]
        [MaxLength(126)]
        public string DoctorId { get; set; }

        [Required]
        [MaxLength(126)]
        public string AppointmentTime { get; set; }

        [Required]
        [MaxLength(126)]
        public string AppointmentDate { get; set; }

        [Required]
        [MaxLength(24)]
        public string AppointmentDayOfWeek { get; set; }

        public bool Active { get; set; } = true;
        public DateTime Added { get; set; }

        [NotMapped]
        public int? AppointmentTimeHour
        { get { return Convert.ToInt32(AppointmentTime.Split(".")[0]); } }

        [NotMapped]
        public SearchAppointmentsForUserConfig? TaskConfigForFiltering { get; set; }

        public void Disable()
        {
            Active = false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NpcpnAppointment);
        }

        public bool Equals(NpcpnAppointment other)
        {
            return other is not null &&
                   DoctorName.Trim() == other.DoctorName.Trim() &&
                   DoctorId.Trim() == other.DoctorId.Trim() &&
                   AppointmentTime.Trim() == other.AppointmentTime.Trim() &&
                   AppointmentDate.Trim() == other.AppointmentDate.Trim() &&
                   AppointmentDayOfWeek.Trim() == other.AppointmentDayOfWeek.Trim();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DoctorName, DoctorId, AppointmentTime, AppointmentDate, AppointmentDayOfWeek);
        }

        public static bool operator ==(NpcpnAppointment left, NpcpnAppointment right)
        {
            return EqualityComparer<NpcpnAppointment>.Default.Equals(left, right);
        }

        public static bool operator !=(NpcpnAppointment left, NpcpnAppointment right)
        {
            return !(left == right);
        }
    }
}