using JobPortalWeb.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JobPortalWeb.Models
{
   
        public class EmployerProfile
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string UserId { get; set; }  // مرتبط با ApplicationUser

            [ForeignKey("UserId")]
            public ApplicationUser User { get; set; }

            [Required, StringLength(150)]
            public string? CompanyName { get; set; }

            [StringLength(500)]
            public string? CompanyDescription { get; set; }

            [StringLength(100)]
            public string? Location { get; set; }

            [StringLength(100)]
            public string? Website { get; set; }

            
        }
}
