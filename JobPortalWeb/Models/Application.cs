using JobPortalWeb.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JobPortalWeb.Models
{
    public class Application
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int JobId { get; set; }

        [ForeignKey("JobId")]
        public Job Job { get; set; }

        [Required]
        public string SeekerId { get; set; }

        [ForeignKey("SeekerId")]
        public ApplicationUser Seeker { get; set; }

        [Required, StringLength(500)]
        public string ResumePath { get; set; }


        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Optional: "Pending", "Reviewed", etc.
    
    }
}
