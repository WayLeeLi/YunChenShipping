using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YunChenShipping.Data;
using YunChenShipping.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// 配置 Cookie 過期時間（7天）
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "YunChenShipping.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // 允許 HTTP
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
});

builder.Services.AddControllersWithViews();

// 配置中文繁體
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

// 配置繁體中文
var supportedCultures = new[] { "zh-TW" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// 初始化角色和管理員帳號
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var seedRoles = new[]
    {
        new ApplicationRole { Name = "Admin", RoleCode = "ADMIN", Description = "系統管理員，擁有所有權限", IsSystem = true, IsActive = true, SortOrder = 1 },
        new ApplicationRole { Name = "Sales", RoleCode = "SALES", Description = "業務人員，管理客戶與出貨單", IsSystem = true, IsActive = true, SortOrder = 2 },
        new ApplicationRole { Name = "Accounting", RoleCode = "ACCOUNTING", Description = "會計人員，管理帳務相關", IsSystem = true, IsActive = true, SortOrder = 3 },
        new ApplicationRole { Name = "Warehouse", RoleCode = "WAREHOUSE", Description = "倉庫人員，管理出貨作業", IsSystem = true, IsActive = true, SortOrder = 4 },
    };
    foreach (var role in seedRoles)
    {
        var roleExist = await roleManager.RoleExistsAsync(role.Name!);
        if (!roleExist)
        {
            await roleManager.CreateAsync(role);
        }
    }

    // 建立預設管理員帳號
    string adminEmail = "admin@yunchen.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
