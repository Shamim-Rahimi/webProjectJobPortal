using JobPortalWeb.Areas.Identity.Data;
using JobPortalWeb.Data;
using JobPortalWeb.Models;
using JobPortalWeb.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

namespace JobPortalWeb.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly DBJobPortalweb _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationController(DBJobPortalweb db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ApplicationViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == vm.JobId);
            if (job == null) return NotFound();

            
            string resumePath = "";
            if (vm.ResumeFile != null && vm.ResumeFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(vm.ResumeFile.FileName);
                var savePath = Path.Combine("wwwroot/resumes", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await vm.ResumeFile.CopyToAsync(stream);
                }
                resumePath = "/resumes/" + fileName;
            }

            
            var application = new Application
            {
                JobId = vm.JobId,
                SeekerId = user.Id,
                ResumePath = resumePath,
                AppliedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            _db.Applications.Add(application);
            await _db.SaveChangesAsync();

            TempData["Success"] = "درخواست شما با موفقیت ارسال شد.";
            return RedirectToAction("MyApplications", "Application");
        }

        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var applications = await _db.Applications
                .Include(a => a.Job) 
                .ThenInclude(j => j.Employer) 
                .Where(a => a.SeekerId == user.Id)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var application = await _db.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.SeekerId == user.Id);

            if (application == null) return NotFound();

            // فقط اگر در وضعیت "Pending" باشد اجازه حذف دارد
            if (application.Status != "Pending")
            {
                TempData["Error"] = "درخواست شما دیگر قابل حذف نیست.";
                return RedirectToAction("MyApplications");
            }

            _db.Applications.Remove(application);
            await _db.SaveChangesAsync();

            TempData["Success"] = "درخواست با موفقیت حذف شد.";
            return RedirectToAction("MyApplications");
        }

        [HttpGet]
        public async Task<IActionResult> EmployerApplications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var applications = await _db.Applications
                .Include(a => a.Job)
                .Include(a => a.Seeker) 
                .Where(a => a.Job.EmployerId == user.Id)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var employer = await _userManager.GetUserAsync(User);
            if (employer == null)
                return RedirectToAction("Login", "Account");

            var application = await _db.Applications
                .Include(a => a.Job)
                .Include(a => a.Seeker) // برای دسترسی به ایمیل کارجو
                .FirstOrDefaultAsync(a => a.Id == id && a.Job.EmployerId == employer.Id);

            if (application == null)
                return NotFound();

            if (status != "Accepted" && status != "Rejected")
            {
                TempData["Error"] = "وضعیت نامعتبر است.";
                return RedirectToAction("EmployerApplications");
            }

            // تغییر وضعیت
            application.Status = status;
            await _db.SaveChangesAsync();

            // اگر درخواست قبول شد، ایمیل ارسال شود
            if (status == "Accepted")
            {
                try
                {
                    var seeker = application.Seeker;

                    MailMessage mailMessage = new MailMessage("shamim.rahimi201@gmail.com", seeker.Email);
                    mailMessage.Subject = "نتیجه درخواست شما در JobPortal";
                    mailMessage.IsBodyHtml = true;

                    mailMessage.Body = $@"
                <h3>سلام {seeker.FirstName} عزیز،</h3>
                <p>درخواست شما برای موقعیت شغلی <b>{application.Job.Title}</b> پذیرفته شد 🎉</p>
                <p>کارفرما به زودی با شما تماس خواهد گرفت.</p>
                <br/>
                <p>با احترام،</p>
                <p><b>Job Portal Team</b></p>
            ";

                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new NetworkCredential("shamim.rahimi201@gmail.com", "afbx nqta yveh wvix");

                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "خطا در ارسال ایمیل: " + ex.Message;
                }
            }

            TempData["Success"] = $"وضعیت درخواست به '{status}' تغییر کرد.";
            return RedirectToAction("EmployerApplications");
        }


    }
}
