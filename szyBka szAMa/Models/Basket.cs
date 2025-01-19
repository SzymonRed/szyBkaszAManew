using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace szyBka_szAMa.Models
{
    public class Basket
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User User { get; set; }
        public List<Dish> Dishes { get; } = new List<Dish>();

    }
}
