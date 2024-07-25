using System.ComponentModel.DataAnnotations;

namespace CaffeeCoApp.Models
{
    public class ProductDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]
        public string Brand { get; set; } = "";
        [Required, MaxLength(100)]
        public string Category { get; set; } = "";
        [Required]
        public decimal Price { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int Stock { get; set; }
        [Required]
        public string Description { get; set; } = "";
        public IFormFile? ImageFile { get; set; }
    }
}
