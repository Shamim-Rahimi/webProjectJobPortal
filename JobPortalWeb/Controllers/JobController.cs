using JobPortalWeb.Areas.Identity.Data;
using JobPortalWeb.Data;
using JobPortalWeb.Models;
using JobPortalWeb.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobPortalWeb.Controllers
{
    public class JobController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly DBJobPortalweb db;

        public JobController(UserManager<ApplicationUser> _userManager, DBJobPortalweb _db)
        {
            this.userManager = _userManager;
            this.db = _db;
        }

        // لیست همه آگهی‌ها - بدون محدودیت
        [HttpGet]
        public IActionResult Index()
        {
            var jobs = db.Jobs.ToList();
            return View(jobs);
        }

        // GET: فرم ایجاد آگهی (کارفرما یا Admin)
        [HttpGet]
        public async Task<IActionResult> CreateJob()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null || (user.IsEmployer != true && !User.IsInRole("Admin")))
            {
                TempData["Error"] = "شما اجازه دسترسی به این صفحه را ندارید.";
                return RedirectToAction("Index");
            }

            return View();
        }

        // POST: ایجاد آگهی
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJob(CreateJobViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null || (user.IsEmployer != true && !User.IsInRole("Admin")))
            {
                TempData["Error"] = "شما اجازه ثبت آگهی را ندارید.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
                return View(model);

            var job = new Job
            {
                Title = model.Title,
                Description = model.Description,
                Location = model.Location,
                SalaryMin = model.SalaryMin,
                SalaryMax = model.SalaryMax,
                JobType = model.JobType,
                ExperienceLevel = model.ExperienceLevel,
                EmployerId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            db.Jobs.Add(job);
            await db.SaveChangesAsync();

            TempData["Success"] = "آگهی با موفقیت ثبت شد.";
            return RedirectToAction("Index");
        }

        // GET: ویرایش آگهی
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var job = await db.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            var user = await userManager.GetUserAsync(User);
            if (user == null || (job.EmployerId != user.Id && !User.IsInRole("Admin") && user.IsEmployer != true))
            {
                TempData["Error"] = "شما اجازه ویرایش این آگهی را ندارید.";
                return RedirectToAction("Index");
            }

            var model = new EditJobViewModel
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                JobType = job.JobType,
                ExperienceLevel = job.ExperienceLevel
            };

            return View(model);
        }

        // POST: ویرایش آگهی
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditJobViewModel model)
        {
            var job = await db.Jobs.FindAsync(model.Id);
            if (job == null) return NotFound();

            var user = await userManager.GetUserAsync(User);
            if (user == null || (job.EmployerId != user.Id && !User.IsInRole("Admin") && user.IsEmployer != true))
            {
                TempData["Error"] = "شما اجازه ویرایش این آگهی را ندارید.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
                return View(model);

            job.Title = model.Title;
            job.Description = model.Description;
            job.Location = model.Location;
            job.SalaryMin = model.SalaryMin;
            job.SalaryMax = model.SalaryMax;
            job.JobType = model.JobType;
            job.ExperienceLevel = model.ExperienceLevel;

            db.Jobs.Update(job);
            await db.SaveChangesAsync();

            TempData["Success"] = "آگهی با موفقیت به‌روزرسانی شد.";
            return RedirectToAction("Index");
        }

        // POST: حذف آگهی
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await db.Jobs.FindAsync(id);
            if (job == null)
            {
                TempData["Error"] = "آگهی یافت نشد.";
                return RedirectToAction("Index");
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null || (job.EmployerId != user.Id && !User.IsInRole("Admin") && user.IsEmployer != true))
            {
                TempData["Error"] = "شما اجازه حذف این آگهی را ندارید.";
                return RedirectToAction("Index");
            }

            db.Jobs.Remove(job);
            await db.SaveChangesAsync();

            TempData["Success"] = "آگهی با موفقیت حذف شد.";
            return RedirectToAction("Index");
        }
    }
}
