using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Legal_Law_Transactions.Models
{
    public class Case
    {
        [Key]
        public int case_id { get; set; }

        public int user_id { get; set; }

        [ForeignKey("user_id")]
        public User User { get; set; }
        public int assigned_to_lawyer { get; set; }
        public int assigned_to_citizen { get; set; }

        public string case_number { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public DateTime court_date { get; set; }
        public DateTime created_at { get; set; }
    }
}