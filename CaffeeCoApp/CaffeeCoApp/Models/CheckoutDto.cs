using System.ComponentModel.DataAnnotations;

namespace CaffeeCoApp.Models
{
    public class CheckoutDto
    {
        [Required(ErrorMessage = "Delivery Address is required")]
        [MaxLength(200)]
        public string ShippingAddress { get; set; } = "";
        public bool IsPickUp { get; set; }
        [Required(ErrorMessage = "The Date you selected is not currently available.")]
        [DataType(DataType.Date)]
        public DateTime DeliveryDate { get; set; }
        public int? StoreId { get; set; }

    }
}
