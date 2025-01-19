using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace szyBka_szAMa.Models
{
    public enum DishCategory
    {
        SideDish,
        MainDish,
        SoftDrink,
        Drink
    }
    public class Dish
    {
        [Key]
        public int Id {  get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string? Photo {  get; set; }
        [Required]
        public float Price {  get; set; }
        [Required]
        public DishCategory DishCategory { get; set; }
        [ForeignKey(nameof(Restaurant))]
        public int RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; }
        public ICollection<Basket>? Baskets { get; } = new List<Basket>();
        public ICollection<Order>? Orders { get; } = new List<Order>();
        public ICollection<DishReview>? Reviews { get; } = new List<DishReview>();
    }
}
