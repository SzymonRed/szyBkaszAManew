using System.ComponentModel.DataAnnotations;

namespace szyBka_szAMa.Models
{
    public class DishReview
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public float Rating { get; set; }
        public string Comment { get; set; }
        public int DishId { get; set; }
        public Dish Dish { get; set; } = null!;
    }
}
