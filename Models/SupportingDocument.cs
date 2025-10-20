using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contract_Monthly_Claim_System_Part2.Models
{
    public class SupportingDocument
    {
        [Key]
        public int SupportingDocumentID { get; set; }

        [Required]
        public int ClaimID { get; set; }

        [ForeignKey("ClaimID")]
        public Claim? Claim { get; set; }

        [Required]
        [StringLength(255)]
        public string? FileName { get; set; }

        [Required]
        public byte[]? EncryptedFile { get; set; }
    }
}