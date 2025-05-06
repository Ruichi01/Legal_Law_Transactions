using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Legal_Law_Transactions.Models
{
    public class Application
    {
        [Key]
        public int application_id { get; set; }

        [Required]
        public string file_path { get; set; }

        public DateTime submitted_at { get; set; }

        [Required]
        public string status { get; set; } = "Pending";

        public string feedback { get; set; } = string.Empty;

        [ForeignKey("User")]
        public int user_id { get; set; }
        public User User { get; set; }
    }
}
