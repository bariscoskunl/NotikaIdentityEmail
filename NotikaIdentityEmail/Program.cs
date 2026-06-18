using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.EmailModels;
using NotikaIdentityEmail.Models.IdentityModels;
using NotikaIdentityEmail.Models.JwtModels;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<EmailContext>().AddErrorDescriber<CustomIdentityValidator>().AddDefaultTokenProviders(); // Burada Identity için gerekli servisleri ekliyoruz. AppUser ve IdentityRole tiplerini kullanıyoruz. Ayrıca Entity Framework ile EmailContext'i kullanarak veritabanı işlemlerini yapıyoruz. CustomIdentityValidator ile özel doğrulama kurallarını ekliyoruz. DataProtectorTokenProvider ile token üretimi için gerekli sağlayıcıyı ekliyoruz.
// AddEntityFrameworkStores IDentity sistemini veritabaniyla calisabilmesi icin gerekli baglantiyi kurar.
// AddErrorDescriber , CustomIdentityValidator icine yazidimiz mesajlar icin 

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<EmailContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<JwtSettingsModel>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<EmailSettingsModel>(builder.Configuration.GetSection("EmailSettings"));

// =========================================================================
// JWT AYARLARINI SİLDİK VE YERİNE STANDART IDENTITY COOKIE AYARLARINI EKLEDİK
// =========================================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    // Eğer kullanıcı giriş yapmadıysa ve yetkili bir sayfaya gitmeye çalışırsa buraya yönlendirilir:
    options.LoginPath = "/Login/UserLogin";

    // Yetkisi yetmeyenlerin yönlendirileceği sayfa:
    options.AccessDeniedPath = "/Error/403";

    // Tarayıcı çerezinin ömrü (Örn: 60 dakika)
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

    // Kullanıcı tarayıcıyı kapatsa bile RememberMe seçeneğiyle çerezin kalıcı olmasını sağlar
    options.SlidingExpiration = true;
});
// =========================================================================

var app = builder.Build();

app.UseStatusCodePagesWithReExecute("/Error/{0}"); 

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
