using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Legal_Law_Transactions.Models
{
    public class AdminLog
    {
        [Key]
        public int log_id { get; set; }

        public int adminId { get; set; } 

        [ForeignKey("adminId")]
        public User Admin { get; set; }  

        public string Action { get; set; }
        public string Target { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
