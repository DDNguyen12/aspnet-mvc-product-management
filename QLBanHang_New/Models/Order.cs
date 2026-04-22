using System;
using System.Collections.Generic;

namespace QLBanHang_New.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        // 🔥 TRẠNG THÁI ĐƠN
        // Chờ xử lý | Đã xử lý | Đã hủy
        public string? Status { get; set; }

        // 🔥 PHƯƠNG THỨC THANH TOÁN (QUAN TRỌNG)
        // COD | QR
        public string? PaymentMethod { get; set; }

        // ===== RELATION =====
        public User? User { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }

        public ICollection<Payment>? Payments { get; set; }

        public ICollection<Shipment>? Shipments { get; set; }
    }
}