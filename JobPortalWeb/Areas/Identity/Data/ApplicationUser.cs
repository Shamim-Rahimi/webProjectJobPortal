using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobPortalWeb.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;

namespace JobPortalWeb.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public bool? IsEmployer { get; set; }
    public bool? IsSeeker { get; set; }



    // اگر کارفرما باشد، لیست Jobهایی که ارسال کرده
    public ICollection<Job>? PostedJobs { get; set; }

    // اگر جویای کار باشد، لیست Application‌هایی که ارسال کرده
    public ICollection<Application>? Applications { get; set; }

    public DateTime? registerTokenExpireTime { get; set; }
}

