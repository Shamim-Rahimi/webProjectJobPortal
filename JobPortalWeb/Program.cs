using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobPortalWeb.Data;
using JobPortalWeb.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DBJobPortalwebConnection") ?? throw new InvalidOperationException("Connection string 'DBJobPortalwebConnection' not found.");

builder.Services.AddSession(x =>
{
    x.IdleTimeout = TimeSpan.FromSeconds(60);
}); 

builder.Services.AddDbContext<DBJobPortalweb>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()   
    .AddEntityFrameworkStores<DBJobPortalweb>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

await CreateRolesAndAdminUser(app);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


async Task CreateRolesAndAdminUser(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // ??? ??? Admin ???? ?????? ????
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // ???? ???? ?? ????? ???? ?????
    var adminEmail = "admin@site.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "????",
            LastName = "??"
        };
        await userManager.CreateAsync(adminUser, "Admin@123"); // ????? ???????
    }

    // ????? ???? ??? Admin ?? ?????
    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
