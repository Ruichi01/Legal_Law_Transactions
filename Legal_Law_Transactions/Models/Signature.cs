using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Legal_Law_Transactions.Models
{
    public class Signature
    {
        [Key]
        public int signature_id { get; set; }

        public int user_id { get; set; }
        public int document_id { get; set; }

        public string signature_hash { get; set; }
        public DateTime signed_at { get; set; }

        [ForeignKey("user_id")]
        public User User { get; set; }

        [ForeignKey("document_id")]
        public Document Document { get; set; }
    }
}
