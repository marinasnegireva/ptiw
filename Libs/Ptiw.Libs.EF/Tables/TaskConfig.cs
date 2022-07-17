namespace Ptiw.Libs.EF.Tables
{
    /// <summary>
    /// Configuration of tasks
    /// </summary>
    public class TaskConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Config { get; set; }

        [Required]
        public long BelongsToUser { get; set; }

        [Required]
        public DateTime Added { get; set; }

        [Required]
        public bool Enabled { get; set; } = true;

        [Required]
        [MaxLength(200)]
        public string JobTypeFullName { get; set; }
    }
}