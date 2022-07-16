namespace Ptiw.Libs.EF.Tables
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(126)]
        public string Username { get; set; }

        [Required]
        public long TelegramId { get; set; }
    }
}