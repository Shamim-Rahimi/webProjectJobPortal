using JobPortalWeb.Areas.Identity.Data;
using JobPortalWeb.Data;
using JobPortalWeb.Models;
using JobPortalWeb.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortalWeb.Controllers
{
    public class SeekerProfileController : Controller
    {
        private readonly DBJobPortalweb db;
        private readonly UserManager<ApplicationUser> _userManager;

        public SeekerProfileController(DBJobPortalweb db, UserManager<ApplicationUser> userManager)
        {
            this.db = db;
            _userManager = userManager;
        }

        // GET
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await db.SeekerProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (profile == null) return NotFound();

            var vm = new SeekerProfileViewModel
            {
                Id = profile.Id,
                FullName = profile.FullName,
                Email = profile.Email,
                Education = profile.Education,
                Experience = profile.Experience,
                Skills = profile.Skills
            };

            return View(vm);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SeekerProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await db.SeekerProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (profile == null) return NotFound();

            profile.FullName = vm.FullName;
            profile.Education = vm.Education;
            profile.Experience = vm.Experience;
            profile.Skills = vm.Skills;

            db.Update(profile);
            await db.SaveChangesAsync();

            TempData["Success"] = "پروفایل با موفقیت به‌روز شد.";
            return RedirectToAction(nameof(Edit));
        }

    
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var profile = await db.SeekerProfiles
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == id);

            if (profile == null)
                return NotFound();

            var vm = new SeekerProfileViewModel
            {
                Id = profile.Id,
                FullName = profile.FullName,
                Email = profile.Email,
                Education = profile.Education,
                Experience = profile.Experience,
                Skills = profile.Skills
            };

            return View(vm);
        }

    }
}
