using JobPortalWeb.Areas.Identity.Data;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;

namespace JobPortalWeb.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; }

        [Required, StringLength(1000)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Location { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalaryMin { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalaryMax { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public string JobType { get; set; }  // full_time, part_time, contract
        public string ExperienceLevel { get; set; } // entry, mid, senior

        [Required]
        public string EmployerId { get; set; }

        [ForeignKey("EmployerId")]
        public ApplicationUser Employer { get; set; }

        public ICollection<Application> Applications { get; set; }

        public bool IsApproved { get; set; } = false;
    }

}
