namespace JobPortalWeb.ViewModels
{
    public class ApplicationViewModel
    {
        public int JobId { get; set; }   // به ویو پاس داده میشه
        public string JobTitle { get; set; }   // فقط برای نمایش توی فرم
        public IFormFile ResumeFile { get; set; }  // فایل رزومه از سمت کاربر
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }
}
