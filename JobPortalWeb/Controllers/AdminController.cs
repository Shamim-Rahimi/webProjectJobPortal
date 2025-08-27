using JobPortalWeb.Areas.Identity.Data;
using JobPortalWeb.Data;
using JobPortalWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace JobPortalWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DBJobPortalweb _db;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            DBJobPortalweb db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        // لیست کاربران + فیلتر ساده
        public async Task<IActionResult> Users(string? q, string? type) // type: "employer" | "seeker" | null
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(u =>
                    u.Email!.Contains(q) ||
                    u.UserName!.Contains(q) ||
                    u.FirstName!.Contains(q) ||
                    u.LastName!.Contains(q));
            }

            if (type == "employer")
                query = query.Where(u => u.IsEmployer == true);
            else if (type == "seeker")
                query = query.Where(u => u.IsSeeker == true);

            var list = await query.ToListAsync();

            
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.Employers = await _userManager.Users.CountAsync(u => u.IsEmployer == true);
            ViewBag.Seekers = await _userManager.Users.CountAsync(u => u.IsSeeker == true);

            
            var allUsers = await _userManager.Users.ToListAsync();
            int adminsCount = 0;
            foreach (var u in allUsers)
            {
                if (await _userManager.IsInRoleAsync(u, "Admin"))
                    adminsCount++;
            }
            ViewBag.Admins = adminsCount;

           
            var result = new List<AdminUserListItemVM>();
            foreach (var u in list)
            {
                result.Add(new AdminUserListItemVM
                {
                    Id = u.Id,
                    Email = u.Email!,
                    FullName = $"{u.FirstName} {u.LastName}".Trim(),
                    IsEmployer = u.IsEmployer == true,
                    IsSeeker = u.IsSeeker == true,
                    IsBanned = u.LockoutEnd.HasValue && u.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow,
                    IsAdmin = await _userManager.IsInRoleAsync(u, "Admin")
                });
            }

            return View(result);
        }


        // Ban / Unban با Lockout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBan(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var isBanned = user.LockoutEnd.HasValue && user.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow;
            if (isBanned)
            {
                user.LockoutEnd = null; // Unban
                TempData["Success"] = "کاربر از حالت مسدود خارج شد.";
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // Ban
                TempData["Success"] = "کاربر مسدود شد.";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Users));
        }

        // حذف کاربر + پاکسازی داده‌های مرتبط (ساده/ایمن)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) { TempData["Error"] = "کاربر یافت نشد."; return RedirectToAction(nameof(Users)); }

            // پاک کردن رکوردهای مرتبط (در صورت نبود Cascade)
            var seekerApps = await _db.Applications.Where(a => a.SeekerId == user.Id).ToListAsync();
            _db.Applications.RemoveRange(seekerApps);

            var jobs = await _db.Jobs
                .Where(j => j.EmployerId == user.Id)
                .Include(j => j.Applications)
                .ToListAsync();

            foreach (var j in jobs)
                _db.Applications.RemoveRange(j.Applications);

            _db.Jobs.RemoveRange(jobs);

            var sp = await _db.SeekerProfiles.SingleOrDefaultAsync(p => p.UserId == user.Id);
            if (sp != null) _db.SeekerProfiles.Remove(sp);

            var ep = await _db.EmployerProfiles.SingleOrDefaultAsync(p => p.UserId == user.Id);
            if (ep != null) _db.EmployerProfiles.Remove(ep);

            await _db.SaveChangesAsync();

            var del = await _userManager.DeleteAsync(user);
            TempData[del.Succeeded ? "Success" : "Error"] = del.Succeeded ? "کاربر حذف شد." : "خطا در حذف کاربر.";
            return RedirectToAction(nameof(Users));
        }

        // تغییر نقش‌های کاری (Boolها)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetEmployer(string id, bool make)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsEmployer = make;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = make ? "کاربر به کارفرما تبدیل شد." : "کاربر از نقش کارفرما خارج شد.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetSeeker(string id, bool make)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsSeeker = make;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = make ? "کاربر به جویای کار تبدیل شد." : "کاربر از نقش جویای کار خارج شد.";
            return RedirectToAction(nameof(Users));
        }

        // افزودن/حذف نقش Admin (Role واقعی)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.AddToRoleAsync(user, "Admin");
            TempData["Success"] = "نقش ادمین به کاربر داده شد.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // جلوگیری از حذف نقش از خودش (اختیاری)
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "نمی‌توانید نقش ادمین را از حساب خودتان بردارید.";
                return RedirectToAction(nameof(Users));
            }

            await _userManager.RemoveFromRoleAsync(user, "Admin");
            TempData["Success"] = "نقش ادمین از کاربر برداشته شد.";
            return RedirectToAction(nameof(Users));
        }

        // لیست همه آگهی‌ها
        public async Task<IActionResult> Jobs(string? q, bool? approved)
        {
            var query = _db.Jobs
                .Include(j => j.Employer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(j =>
                    j.Title.Contains(q) ||
                    j.Description.Contains(q) ||
                    j.Location.Contains(q) ||
                    j.Employer.Email.Contains(q));
            }

            if (approved.HasValue)
                query = query.Where(j => j.IsApproved == approved.Value);

            var list = await query
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            // 📊 آمار آگهی‌ها
            ViewBag.TotalJobs = await _db.Jobs.CountAsync();
            ViewBag.ApprovedJobs = await _db.Jobs.CountAsync(j => j.IsApproved == true);
            ViewBag.PendingJobs = await _db.Jobs.CountAsync(j => j.IsApproved == false);

            return View(list);
        }

        // تأیید آگهی
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveJob(int id)
        {
            var job = await _db.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            job.IsApproved = true;
            await _db.SaveChangesAsync();

            TempData["Success"] = "آگهی تأیید شد.";
            return RedirectToAction(nameof(Jobs));
        }

        // رد آگهی
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectJob(int id)
        {
            var job = await _db.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            job.IsApproved = false;
            await _db.SaveChangesAsync();

            TempData["Success"] = "آگهی رد شد.";
            return RedirectToAction(nameof(Jobs));
        }

        // فعال/غیرفعال کردن
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var job = await _db.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            job.IsActive = !job.IsActive;
            await _db.SaveChangesAsync();

            TempData["Success"] = job.IsActive ? "آگهی فعال شد." : "آگهی غیرفعال شد.";
            return RedirectToAction(nameof(Jobs));
        }

        // حذف آگهی
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _db.Jobs
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null) return NotFound();

            // پاک کردن اپلیکیشن‌های مرتبط
            _db.Applications.RemoveRange(job.Applications);

            _db.Jobs.Remove(job);
            await _db.SaveChangesAsync();

            TempData["Success"] = "آگهی حذف شد.";
            return RedirectToAction(nameof(Jobs));
        }

        [HttpGet]
        public async Task<IActionResult> Applications(string status = "")
        {
            var query = _db.Applications
                .Include(a => a.Job)
                .ThenInclude(j => j.Employer)
                .Include(a => a.Seeker)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            var applications = await query
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            
            ViewBag.Total = await _db.Applications.CountAsync();
            ViewBag.Pending = await _db.Applications.CountAsync(a => a.Status == "Pending");
            ViewBag.Accepted = await _db.Applications.CountAsync(a => a.Status == "Accepted");
            ViewBag.Rejected = await _db.Applications.CountAsync(a => a.Status == "Rejected");

            return View(applications);
        }

    }
}
