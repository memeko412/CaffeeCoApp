using Microsoft.EntityFrameworkCore;

namespace CaffeeCoApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = "";
        public AppUser Client { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        [Precision(16,2)]
        public bool IsPickUp { get; set; }
        public string ShippingAddress { get; set; } = "";
        public string ShippingStatus { get; set; } = "";
        public DateOnly DeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public Store? Store { get; set; }
    }
}
