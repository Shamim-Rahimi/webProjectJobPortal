using System.ComponentModel.DataAnnotations;

namespace JobPortalWeb.ViewModels
{
    public class SeekerProfileViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string? Education { get; set; }
        public string? Experience { get; set; }
        public string? Skills { get; set; }
    }
}
