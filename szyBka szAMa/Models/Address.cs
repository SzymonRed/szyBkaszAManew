using System.ComponentModel.DataAnnotations;

namespace szyBka_szAMa.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string ZipCode { get; set; }

        [Required]
        public string Building { get; set; }

        public string? Apartment { get; set; }

        [Required]
        public string Email { get; set; }

        public string? Phone { get; set; }
    }
}