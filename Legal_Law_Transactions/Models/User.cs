using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Legal_Law_Transactions.Models
{
    public class User
    {
        [Key]
        public int user_id { get; set; }

        public string lastname { get; set; }
        public string firstname { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string role { get; set; }

        // Navigation properties
        public ICollection<Record> Records { get; set; } = new List<Record>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Signature> Signatures { get; set; } = new List<Signature>();
        public ICollection<Case> Cases { get; set; } = new List<Case>();
        public ICollection<License> Licenses { get; set; } = new List<License>();
        public ICollection<Evidence> Evidences { get; set; } = new List<Evidence>();
    }
}
