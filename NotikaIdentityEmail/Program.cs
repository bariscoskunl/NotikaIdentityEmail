using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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

// 1. IDENTITY & DATABASE CONFIGURATION
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<EmailContext>()
    .AddErrorDescriber<CustomIdentityValidator>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<EmailContext>(options => options.UseSqlServer(connectionString));

// 2. CONFIGURATION BINDING (IOptions Pattern)
builder.Services.Configure<JwtSettingsModel>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<EmailSettingsModel>(builder.Configuration.GetSection("EmailSettings"));

// 3. GÖRSELDEKİ HYBRID AUTHENTICATION MIMARISI (Cookie + JWT)
builder.Services.AddAuthentication(options =>
{
    // ÖĞRENME NOTU: Görseldeki gibi varsayılan şemayı Cookie yapıyoruz.
    // Böylece normal MVC sayfaların (Profile, Message vb.) tarayıcı çerezinden sorunsuz okunur, 'null' hatası vermez.
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})

.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
{
    // ÖĞRENME NOTU: Kaldırmak istemediğimiz, dersin asıl konusu olan JWT doğrulaması burada aktif kalıyor.
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettingsModel>();
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
}).AddGoogle(GoogleDefaults.AuthenticationScheme, options =>                // Google Authentication Configuration
{
    options.ClientId = builder.Configuration["GoogleLogin:ClientId"];
    options.ClientSecret = builder.Configuration["GoogleLogin:ClientSecret"];
    
    // Google'ın her girişte hesap seçme ekranını zorunlu göstermesi için:
    options.Events.OnRedirectToAuthorizationEndpoint = context =>
    {
        context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
        return Task.CompletedTask;
    };
});





//  AddCookie içindeki LoginPath ayarlarını Identity projesinde burası yönetir:
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login/UserLogin";
    options.AccessDeniedPath = "/Error/403";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});
var app = builder.Build();

// 4. MIDDLEWARE PIPELINE
app.UseStatusCodePagesWithReExecute("/Error/{0}");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ÖĞRENME NOTU: Kimlik doğrulama (Authentication) her zaman yetkilendirmeden (Authorization) önce gelmelidir.
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();