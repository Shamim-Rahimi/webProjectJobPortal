using System.ComponentModel.DataAnnotations;

namespace JobPortalWeb.ViewModels
{
    public class CreateJobViewModel
    {
        [Required(ErrorMessage = "عنوان شغلی الزامی است")]
        [StringLength(150, ErrorMessage = "عنوان نمی‌تواند بیشتر از ۱۵۰ کاراکتر باشد")]
        public string Title { get; set; }

        [Required(ErrorMessage = "توضیحات الزامی است")]
        [StringLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از ۱۰۰۰ کاراکتر باشد")]
        public string Description { get; set; }

        [StringLength(100)]
        public string Location { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "مقدار حداقل حقوق معتبر نیست")]
        public decimal? SalaryMin { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "مقدار حداکثر حقوق معتبر نیست")]
        public decimal? SalaryMax { get; set; }

        [Required(ErrorMessage = "نوع قرارداد الزامی است")]
        public string JobType { get; set; }  // full_time, part_time, contract

        [Required(ErrorMessage = "سطح تجربه الزامی است")]
        public string ExperienceLevel { get; set; } // entry, mid, senior
    }
}
