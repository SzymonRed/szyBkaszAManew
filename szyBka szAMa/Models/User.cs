using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace szyBka_szAMa.Models
{
    public enum UserRole
    {
        SystemAdministrator,
        RestaurantAdministrator,
        RestaurantCook,
        RestaurantDelivery,
        RestaurantWaiter,
        Client
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        [ForeignKey(nameof(Address))]
        public int AddressId { get; set; }
        public Address Address { get; set; }

        public int? RestaurantId { get; set; }
        [ForeignKey(nameof(RestaurantId))]
        public Restaurant? Restaurant { get; set; } // Null for clients or system administrators
    }
}