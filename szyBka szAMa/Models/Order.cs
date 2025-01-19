using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace szyBka_szAMa.Models
{
    public enum DeliveryMethod
    {
        Delivery,
        Pickup
    }
    public enum PaymentMethod
    {
        Online,
        OnDelivery
    }
    public enum OrderStatus
    {
        WaitingPayment,
        Taken,
        InPreparation,
        InDelivery,
        Delivered
    }
    public class Order
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(User))]
        public int UserId {  get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public bool PaymentComplete { get; set; }
        public DateTime TimeOrdered { get; set; }
        public DateTime? TimePrepared { get; set; }
        public DateTime? TimeDelivered { get; set; }
        public OrderStatus Status { get; set; }
        public int RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; }
        public ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    }
}