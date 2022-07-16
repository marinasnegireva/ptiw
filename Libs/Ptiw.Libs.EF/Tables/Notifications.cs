namespace Ptiw.Libs.EF.Tables
{
    /// <summary>
    /// Universal notification data
    /// </summary>
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(4096)]
        public string NotificationText { get; set; }

        [Required]
        public long UserIdTo { get; set; }

        [Required]
        public DateTime Added { get; set; }

        [Required]
        public bool Completed { get; set; } = false;

        public DateTime CompletedTime { get; set; }
    }
}