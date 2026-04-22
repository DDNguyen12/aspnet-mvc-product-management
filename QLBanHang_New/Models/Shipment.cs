using System;

namespace QLBanHang_New.Models
{
    public class Shipment
    {
        public int ShipmentId { get; set; }

        public int OrderId { get; set; }

        public string? Address { get; set; }

        public DateTime? ShipDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public string? Status { get; set; }

        public Order? Order { get; set; }
    }
}