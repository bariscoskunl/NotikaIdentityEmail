using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.IdentityModels;

namespace NotikaIdentityEmail.Extensions
{
    public static class DatabaseAndIdentityExtensions
    {
        public static IServiceCollection AddDatabaseAndIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Veritabanı Bağlantısı Ayarları
            var connectionString = configuration.GetConnectionString("Default");
            services.AddDbContext<EmailContext>(options => options.UseSqlServer(connectionString));

            // 2. Identity Ayarları
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<EmailContext>()
                .AddErrorDescriber<CustomIdentityValidator>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}
