using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortalWeb.Areas.Identity.Data;

namespace JobPortalWeb.Models
{
    public class SeekerProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }  // مرتبط با ApplicationUser

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; }  // از ApplicationUser پر می‌شود

        [Required, EmailAddress]
        public string Email { get; set; }  // از ApplicationUser پر می‌شود

        [StringLength(500)]
        public string? Education { get; set; }

        [StringLength(500)]
        public string? Experience { get; set; }

        [StringLength(500)]
        public string? Skills { get; set; }

        
    }
}
