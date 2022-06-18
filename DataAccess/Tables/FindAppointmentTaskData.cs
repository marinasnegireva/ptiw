using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ptiw.DataAccess.Tables
{
    /// <summary>
    /// Class for result data
    /// </summary>
    public class FindAppointmentTaskData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(126)]
        public string DoctorName { get; set; }

        public string DoctorId { get; set; }

        [Required]
        public string AppointmentTime { get; set; }

        public string AppointmentDate { get; set; }
        public string AppointmentDayOfWeek { get; set; }
        public bool Notified { get; set; } = false;
        public DateTime Added { get; set; }
    }
}