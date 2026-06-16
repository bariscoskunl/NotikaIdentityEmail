using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.IdentityModels;
using NotikaIdentityEmail.Models.JwtModels;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<EmailContext>().AddErrorDescriber<CustomIdentityValidator>();
// AddEntityFrameworkStores IDentity sistemini veritabaniyla calisabilmesi icin gerekli baglantiyi kurar.
// AddErrorDescriber , CustomIdentityValidator icine yazidimiz mesajlar icin 

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<EmailContext>(options =>
options.UseSqlServer(connectionString));

builder.Services.Configure<JwtSettingsModel>(builder.Configuration.GetSection("JwtSettings")); 


builder.Services.AddAuthentication(options => // Authentication davranışlarını yapılandırmak için kullanılır. Bu, uygulamanın nasıl kimlik doğrulama yapacağını belirler.
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Uygulamanın varsayılan olarak hangi kimlik doğrulama şemasını kullanacağını belirtir.
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Bu, JWT Bearer şeması olarak ayarlanır, yani uygulama JWT token kullanarak kimlik doğrulama yapacaktır.
}).AddJwtBearer(opt => // JWT Bearer kimlik doğrulama şemasını yapılandırmak için kullanılır. 
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettingsModel>(); // Uygulamanın yapılandırma dosyasıni kullanarak JWT ayarlarını alır. // burada appsettings dosyasındaki JwtSettings bölümündeki ayarları JwtSettingsModel sınıfına bind ederiz. !!!!
    opt.TokenValidationParameters = new TokenValidationParameters // JWT token'larının doğrulanması için gerekli parametreleri belirler. 
    {
        ValidateIssuer = true,// Token'ın geçerli bir yayıncı tarafından oluşturulup oluşturulmadığını doğrular. Bu, token'ın güvenilir bir kaynaktan geldiğini garanti eder.
        ValidateAudience = true,// Token'ın geçerli bir hedef kitleye sahip olup olmadığını doğrular. Bu, token'ın belirli bir uygulama veya hizmet için oluşturulduğunu garanti eder.
        ValidateLifetime = true,// Bu, token'ın belirli bir süre boyunca geçerli olduğunu garanti eder.
        ValidateIssuerSigningKey = true,// Bu, token'ın değiştirilmediğini ve güvenilir bir şekilde imzalandığını garanti eder.
        ValidIssuer = jwtSettings.Issuer,// Token'ın geçerli bir yayıncı tarafından oluşturulup oluşturulmadığını doğrulamak için kullanılan yayıncı değerini belirtir.
        ValidAudience = jwtSettings.Audience,// Token'ın geçerli bir hedef kitleye sahip olup olmadığını doğrulamak için kullanılan hedef kitle değerini belirtir. 
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))// Token'ın imzalanması için kullanılan anahtarın geçerli olup olmadığını doğrulamak için kullanılan imzalama anahtarını belirtir.
    };
});


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
