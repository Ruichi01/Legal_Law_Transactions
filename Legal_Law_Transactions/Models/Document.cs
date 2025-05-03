using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Legal_Law_Transactions.Models
{
    public class Document
    {
        [Key]
        public int document_id { get; set; }

        public string title { get; set; }
        public string content { get; set; }
        public string notarized { get; set; } 
        public DateTime created_at { get; set; }
        public DateTime last_modified { get; set; }

        public int user_id { get; set; }
        public User User { get; set; }
        public int assigned_to_lawyer { get; set; }
        public int assigned_to_citizen { get; set; }

        public string file_path { get; set; }
        public ICollection<Signature> Signatures { get; set; } = new List<Signature>();
    }
}
