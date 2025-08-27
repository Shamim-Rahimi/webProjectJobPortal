using System.ComponentModel.DataAnnotations;

namespace JobPortalWeb.ViewModels
{
    public class EmployerProfileViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string? CompanyName { get; set; }

        [StringLength(500)]
        public string? CompanyDescription { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? Website { get; set; }

        public string? Email { get; set; }
    }
}
