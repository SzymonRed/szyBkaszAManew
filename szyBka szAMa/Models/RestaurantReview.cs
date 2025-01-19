using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace szyBka_szAMa.Models
{
    public class RestaurantReview
    {
        [Key]
        public int Id { get; set; }
        public float Rating { get; set; }
        public string Comment { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }

    }
}
