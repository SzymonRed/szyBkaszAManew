using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace szyBka_szAMa.Models
{
    public class UserMessage 
    {
        public int UserId { get; set; }
        public int MessageId { get; set; }
        public Message Message { get; set; }
        [ForeignKey(nameof(Restaurant))]
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
    }
}
