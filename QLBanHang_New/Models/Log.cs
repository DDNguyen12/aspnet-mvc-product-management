using System;

namespace QLBanHang_New.Models
{
    public class Log
    {
        public int LogId { get; set; }

        public int? UserId { get; set; }

        public string? Action { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}