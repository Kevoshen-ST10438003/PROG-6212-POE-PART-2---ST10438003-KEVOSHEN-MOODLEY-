using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contract_Monthly_Claim_System_Part2.Models
{
    public enum ClaimStatus
    {
        Pending,
        CoordinatorApproved,
        ManagerApproved,
        Rejected
    }

    public class Claim
    {
        [Key]
        public int ClaimID { get; set; }

        [Required(ErrorMessage = "Lecturer name is required")]
        [MaxLength(100)]
        public string LecturerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 200, ErrorMessage = "Hours must be between 1 and 200")]
        public int HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(50, 5000, ErrorMessage = "Rate must be between 50 and 5000")]
        public decimal HourlyRate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        public ICollection<SupportingDocument>? Documents { get; set; }
    }
}
