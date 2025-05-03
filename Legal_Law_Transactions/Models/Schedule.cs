using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Legal_Law_Transactions.Models
{
    public class Schedule
    {
        [Key]
        public int schedule_id { get; set; }

        public int case_id { get; set; }

        [ForeignKey("case_id")]
        public Case Case { get; set; }

        public DateTime court_date { get; set; }
        public string courtroom_number { get; set; }
        public string status { get; set; }
        public int assigned_to_lawyer { get; set; }
        public int assigned_to_citizen { get; set; }
    }
}
