using System.ComponentModel.DataAnnotations;

namespace szyBka_szAMa.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime TimeSend { get; set; }
    }
}
