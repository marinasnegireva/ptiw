namespace Ptiw.Libs.EF.Tables
{
    /// <summary>
    /// Class for result data
    /// </summary>
    public class UserNotificationsAggregateLogEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public int FkId { get; set; }

        [Required]
        [MaxLength(126)]
        public string JobName { get; set; }
    }
}