using System.Collections.Generic;

namespace QLBanHang_New.Models
{
    public class Cart
    {
        public int CartId { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}