﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaffeeCoApp.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        [Precision(16,2)]
        public decimal UnitPrice { get; set; }
        public int? Rating { get; set; } = null;

        // navigation property
        public Product Product { get; set; } = new Product();
    }
}
