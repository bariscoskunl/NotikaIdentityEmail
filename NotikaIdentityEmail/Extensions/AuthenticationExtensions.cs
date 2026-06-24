using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NotikaIdentityEmail.Models.JwtModels;
using System.Text;

namespace NotikaIdentityEmail.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // 3. GÖRSELDEKİ HYBRID AUTHENTICATION MIMARISI (Cookie + JWT)
           services.AddAuthentication(options =>
            {
                // ÖĞRENME NOTU: Görseldeki gibi varsayılan şemayı Cookie yapıyoruz.
                // Böylece normal MVC sayfaların (Profile, Message vb.) tarayıcı çerezinden sorunsuz okunur, 'null' hatası vermez.
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
            {
                // ÖĞRENME NOTU: Kaldırmak istemediğimiz, dersin asıl konusu olan JWT doğrulaması burada aktif kalıyor.
                var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettingsModel>();
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
                options.ClientId = configuration["GoogleLogin:ClientId"];
                options.ClientSecret = configuration["GoogleLogin:ClientSecret"];

                // Google'ın her girişte hesap seçme ekranını zorunlu göstermesi için:
                options.Events.OnRedirectToAuthorizationEndpoint = context =>
                {
                    context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
                    return Task.CompletedTask;
                };
            });

            //  AddCookie içindeki LoginPath ayarlarını Identity projesinde burası yönetir:
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login/UserLogin";
                options.AccessDeniedPath = "/Error/403";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
            });

            return services;
        }
    }
}
