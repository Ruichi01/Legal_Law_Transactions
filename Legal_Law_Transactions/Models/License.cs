using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Legal_Law_Transactions.Models
{
    public class License
    {
        [Key]
        public int license_id { get; set; }

        public int user_id { get; set; }

        [ForeignKey("user_id")]
        public User User { get; set; }

        public string type { get; set; }
        public DateTime issue_date { get; set; }
        public DateTime expiry_date { get; set; }
        public string status { get; set; }

        public string license_image { get; set; }
    }
}
