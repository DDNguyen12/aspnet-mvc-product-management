using System;
using System.Collections.Generic;

namespace QLBanHang_New.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public int RoleId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Role? Role { get; set; }

        public ICollection<Cart>? Carts { get; set; }

        public ICollection<Order>? Orders { get; set; }

        public ICollection<Review>? Reviews { get; set; }
    }
}