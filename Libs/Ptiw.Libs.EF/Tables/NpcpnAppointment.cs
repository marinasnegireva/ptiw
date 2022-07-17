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
        public DateTime Appointment { get; set; }

        public bool Active { get; set; } = true;
        public DateTime Added { get; set; }

        [NotMapped]
        public FindAppointmentsConfig? TaskConfigForFiltering { get; set; }

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
                   Appointment == other.Appointment;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DoctorName, DoctorId, Appointment);
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