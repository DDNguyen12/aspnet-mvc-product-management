using System;
using System.Collections.Generic;

namespace QLBanHang_New.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public decimal Price { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int Stock { get; set; }

        public int? CategoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Color { get; set; }

        public Category? Category { get; set; }

        public ICollection<CartItem>? CartItems { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }

        public ICollection<Review>? Reviews { get; set; }
    }
}