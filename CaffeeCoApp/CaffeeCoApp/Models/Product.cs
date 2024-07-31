using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CaffeeCoApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]
        public string Brand { get; set; } = "";
        [MaxLength(100)]
        public string Category { get; set; } = "";
        [Precision(16, 2)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
        public string Description { get; set; } = "";
        [MaxLength(100)]
        public string ImageFileName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        [Range(0, 5)]
        public double Rating { get; set; } = 0;
        [Range(0, int.MaxValue)]
        public int RatingCount { get; set; } = 0;
    }
}
