using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace szyBka_szAMa.Models
{
    public enum DayOfWeek
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }
    public class WorkHour
    {
        [Key]
        public int Id {  get; set; }
        [Required]
        public DayOfWeek DayOfWeek { get; set; }
        [Required]
        public DateTime OpenHour { get; set; }
        [Required]
        public DateTime CloseHour { get; set; }
        [ForeignKey(nameof(Restaurant))]
        public int RestaurantId {  get; set; }
        public Restaurant? Restaurant { get; set; }
    }
}
