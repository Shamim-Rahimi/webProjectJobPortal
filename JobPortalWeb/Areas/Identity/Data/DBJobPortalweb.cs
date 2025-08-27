using JobPortalWeb.Areas.Identity.Data;
using JobPortalWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobPortalWeb.Data;

public class DBJobPortalweb : IdentityDbContext<ApplicationUser>
{
    public DBJobPortalweb(DbContextOptions<DBJobPortalweb> options)
        : base(options)
    {
    }

    public DbSet<Job> Jobs { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<SeekerProfile> SeekerProfiles { get; set; }
    public DbSet<EmployerProfile> EmployerProfiles { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        builder.Entity<Job>()
            .HasOne(j => j.Employer)
            .WithMany(u => u.PostedJobs)
            .HasForeignKey(j => j.EmployerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Application>()
            .HasOne(a => a.Seeker)
            .WithMany(u => u.Applications)
            .HasForeignKey(a => a.SeekerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Application>()
            .HasOne(a => a.Job)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobId)
            .OnDelete(DeleteBehavior.Cascade); // مشکلی نداره چون فقط یک مسیر cascade دا
    }
}
