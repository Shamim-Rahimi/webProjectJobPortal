using System.ComponentModel.DataAnnotations;

namespace JobPortalWeb.ViewModels
{
    public class EditJobViewModel
    {
        [Required]
        public int Id { get; set; }  // برای شناسایی آگهی

        [Required, StringLength(150)]
        [Display(Name = "عنوان شغل")]
        public string Title { get; set; }

        [Required, StringLength(1000)]
        [Display(Name = "توضیحات شغل")]
        public string Description { get; set; }

        [StringLength(100)]
        [Display(Name = "مکان")]
        public string Location { get; set; }

        [Display(Name = "حداقل حقوق")]
        public decimal? SalaryMin { get; set; }

        [Display(Name = "حداکثر حقوق")]
        public decimal? SalaryMax { get; set; }

        [Display(Name = "نوع شغل")]
        public string JobType { get; set; }  // full_time, part_time, contract

        [Display(Name = "سطح تجربه")]
        public string ExperienceLevel { get; set; } // entry, mid, senior
    }
}
