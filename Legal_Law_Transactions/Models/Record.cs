using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Legal_Law_Transactions.Models
{
    public class Record
    {
        [Key]
        public int record_id { get; set; }

        public int user_id { get; set; }
        public int case_id { get; set; }

        public string offense_type { get; set; }
        public DateTime date_of_offense { get; set; }
        public string status { get; set; }
        public DateTime last_updated { get; set; }

        [ForeignKey("user_id")]
        public User User { get; set; }

        [ForeignKey("case_id")]
        public Case Case { get; set; }
    }
}