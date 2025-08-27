using JobPortalWeb.Areas.Identity.Data;
using JobPortalWeb.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using JobPortalWeb.Data;
using JobPortalWeb.Models;

namespace JobPortalWeb.Controllers
{
    public class AccountController : Controller
    {
        SignInManager<ApplicationUser> signInManager;
        UserManager<ApplicationUser> userManager;
        DBJobPortalweb db;

        public AccountController(UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, DBJobPortalweb _db)
        {
            this.signInManager = _signInManager;
            this.userManager = _userManager;
            this.db = _db;
        }

        
        public IActionResult Register()
        {
            return View();
        }

        
        public async Task<IActionResult> RegisterConfirm(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser()
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    IsEmployer = model.Role == "Employer",
                    IsSeeker = model.Role == "Seeker",
                    registerTokenExpireTime = DateTime.Now.AddMinutes(2)
                };

                IdentityResult result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (user.IsSeeker == true)
                    {
                        var seeker = new SeekerProfile
                        {
                            UserId = user.Id,
                            FullName = $"{user.FirstName} {user.LastName}",
                            Email = user.Email,
                            Education = "",
                            Experience = "",
                            Skills = ""
                        };
                        db.SeekerProfiles.Add(seeker);
                    }
                    else if (user.IsEmployer == true)
                    {
                        var employer = new EmployerProfile
                        {
                            UserId = user.Id,
                            CompanyName = "",
                            CompanyDescription = "",
                            Location = "",
                            Website = ""
                        };
                        db.EmployerProfiles.Add(employer);
                    }

                    await db.SaveChangesAsync();


                    //Sending Email
                    MailMessage mailMessage = new MailMessage("shamim.rahimi201@gmail.com", user.Email);
                    mailMessage.Subject = " به جاب پرتال خوش آمدید!" +
                        " تایید حساب کاربری ";
                    mailMessage.IsBodyHtml = true;
                    string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    string address = Url.Action("EmailConfirm", "Account", new { id = user.Id, token = token }, "https");

                    mailMessage.Body = $"Hi <b>{user.FirstName}<b>" +
                        $"Please click this <a href='{address}'>LINK</a> to confirm your account";


                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);//465
                    smtpClient.EnableSsl = true;//ramznegari/security
                    smtpClient.Credentials = new NetworkCredential("shamim.rahimi201@gmail.com", "afbx nqta yveh wvix");
                    smtpClient.Send(mailMessage);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                      
                    }

                   
                }
            
            }
            return View("Register", model);
        }
       
        public async Task<IActionResult> EmailConfirm(string id, string token)
        {
            ApplicationUser user = await userManager.FindByIdAsync(id);
            if (user.registerTokenExpireTime >= DateTime.Now)
            {
                await userManager.ConfirmEmailAsync(user, token);
            }
            else
            {
                // TempData["message_token"] = "ٍEmail confirmation token has been expired...!"; ;
            }
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "ایمیل یا رمز عبور اشتباه است");
            }

            return View(model);
        }

      

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

      
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> RestorePassword(ForgetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ForgotPassword", model);
            }

            var user = await userManager.FindByEmailAsync(model.username);
            if (user != null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var forgotPasswordLink = Url.Action("SetResetPassword", "Account",
                    new { token = token, id = user.Id }, protocol: HttpContext.Request.Scheme);

                // ارسال ایمیل
                MailMessage mailMessage = new MailMessage("shamim.rahimi201@gmail.com", user.Email);
                mailMessage.Subject = "تغییر رمز عبور  ";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = $"سلام <b>{user.FirstName}</b>، <br/>" +
                                   $"برای تغییر رمز عبور خود <a href='{forgotPasswordLink}'>اینجا</a> کلیک کنید.";

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new NetworkCredential("shamim.rahimi201@gmail.com", "afbx nqta yveh wvix");
                    smtpClient.Send(mailMessage);
                }
            }

            TempData["msg"] = "اگر ایمیل شما در سیستم وجود داشته باشد، لینک تغییر رمز ارسال خواهد شد.";
            return RedirectToAction("Login", "Account");
        }

       
        [HttpGet]
        public async Task<IActionResult> SetResetPassword(string token, string id,
           [FromServices] UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("ForgotPassword");
            }

            HttpContext.Session.SetString("id", id);
            HttpContext.Session.SetString("token", token);
            ViewData["username"] = user.UserName;

            return View(new ResetPasswordViewModel());
        }

        
        [HttpPost]
        public async Task<IActionResult> FinalRestorePassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("SetResetPassword", model);
            }

            if (model.Pass1 != model.Confirm)
            {
                ModelState.AddModelError("", "رمز عبور و تکرار آن یکسان نیستند");
                return View("SetResetPassword", model);
            }

            string? id = HttpContext.Session.GetString("id");
            string? token = HttpContext.Session.GetString("token");

            if (id == null || token == null)
            {
                return RedirectToAction("ForgotPassword", "Account");
            }

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return RedirectToAction("ForgotPassword", "Account");
            }

            var result = await userManager.ResetPasswordAsync(user, token, model.Pass1);

            if (result.Succeeded)
            {
                TempData["msg"] = "رمز عبور شما با موفقیت تغییر یافت. لطفاً وارد شوید.";
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View("SetResetPassword", model);
        }


    }
}
