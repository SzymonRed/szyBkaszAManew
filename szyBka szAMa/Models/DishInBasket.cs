using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace szyBka_szAMa.Models
{
    public class DishInBasket
    {
        public int BasketId { get; set; }
        public int DishId { get; set; }
        public int Amount { get; set; }
    }
}
