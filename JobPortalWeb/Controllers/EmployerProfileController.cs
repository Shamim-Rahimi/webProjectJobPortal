using JobPortalWeb.Areas.Identity.Data;
using JobPortalWeb.Data;
using JobPortalWeb.Models;
using JobPortalWeb.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortalWeb.Controllers
{
    public class EmployerProfileController : Controller
    {
        private readonly DBJobPortalweb db;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployerProfileController(DBJobPortalweb db, UserManager<ApplicationUser> userManager)
        {
            this.db = db;
            _userManager = userManager;
        }

        
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await db.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == user.Id);
            if (profile == null) return NotFound();

            var vm = new EmployerProfileViewModel
            {
                Id = profile.Id,
                CompanyName = profile.CompanyName,
                CompanyDescription = profile.CompanyDescription,
                Location = profile.Location,
                Website = profile.Website,
                Email = user.Email 
            };

            return View(vm);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployerProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await db.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == user.Id);
            if (profile == null) return NotFound();

            profile.CompanyName = vm.CompanyName;
            profile.CompanyDescription = vm.CompanyDescription;
            profile.Location = vm.Location;
            profile.Website = vm.Website;

            db.Update(profile);
            await db.SaveChangesAsync();

            TempData["Success"] = "پروفایل کارفرما با موفقیت به‌روز شد.";
            return RedirectToAction(nameof(Edit));
        }

        [HttpGet]
        public async Task<IActionResult> Details(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest();

            var profile = await db.EmployerProfiles
                .Include(e => e.User) 
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (profile == null)
                return NotFound();

            return View(profile);
        }

    }
}
