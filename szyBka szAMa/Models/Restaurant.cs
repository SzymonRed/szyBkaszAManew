using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace szyBka_szAMa.Models
{
    public class Restaurant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [ForeignKey(nameof(Address))]
        public int AddressId { get; set; }
        public Address Address { get; set; }

        public ICollection<User> Employees { get; set; } = new List<User>();

        public ICollection<RestaurantReview> Reviews { get; set; } = new List<RestaurantReview>();
        public ICollection<WorkHour> WorkHours { get; set; } = new List<WorkHour>();
        public ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    }
}