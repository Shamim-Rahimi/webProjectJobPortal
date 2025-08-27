using JobPortalWeb.Data;
using JobPortalWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortalWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBJobPortalweb _db;

        public HomeController(DBJobPortalweb db)
        {
            _db = db;
        }

        // صفحه اصلی با قابلیت جستجو
        public async Task<IActionResult> Index(string? title, string? location)
        {
            // گرفتن همه آگهی‌های فعال و تایید شده
            var query = _db.Jobs
                           .Include(j => j.Employer)
                           .Where(j => j.IsActive && j.IsApproved);

            // فیلتر بر اساس عنوان شغل
            if (!string.IsNullOrWhiteSpace(title))
            {
                string t = title.Trim().ToLower();
                query = query.Where(j => j.Title.ToLower().Contains(t));
            }

            // فیلتر بر اساس مکان
            if (!string.IsNullOrWhiteSpace(location))
            {
                string loc = location.Trim().ToLower();
                query = query.Where(j => j.Location.ToLower().Contains(loc));
            }

            // مرتب‌سازی بر اساس تاریخ ایجاد
            var jobs = await query
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            return View(jobs);
        }

        // جزئیات هر آگهی
        public async Task<IActionResult> JobDetails(int id)
        {
            var job = await _db.Jobs
                               .Include(j => j.Employer)
                               .FirstOrDefaultAsync(j => j.Id == id && j.IsActive && j.IsApproved);

            if (job == null)
                return NotFound();

            return View(job);
        }
    }
}
