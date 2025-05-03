using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Legal_Law_Transactions.Models
{
    public class Evidence
    {
        [Key]
        public int evidence_id { get; set; }

        public int case_id { get; set; }
        [ForeignKey("case_id")]
        public Case Case { get; set; }

        public int user_id { get; set; }
        [ForeignKey("user_id")]
        public User User { get; set; }

        public string file_path { get; set; } 

        public string description { get; set; }

        public string status { get; set; } = "Pending";
        public int assigned_to_lawyer { get; set; }
        public int assigned_to_citizen { get; set; }


        public DateTime created_at { get; set; } = DateTime.Now;
    }
}
