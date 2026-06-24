using NotikaIdentityEmail.Models.EmailModels;
using NotikaIdentityEmail.Models.JwtModels;

namespace NotikaIdentityEmail.Extensions
{
    public static class ConfigurationBindingsExtensions
    {
        public static IServiceCollection AddConfigurationBindings(this IServiceCollection services, IConfiguration configuration)
        {
            // 2. Konfigürasyon Bağlamaları
            services.Configure<JwtSettingsModel>(configuration.GetSection("JwtSettings"));
            services.Configure<EmailSettingsModel>(configuration.GetSection("EmailSettings"));
            return services;
        }
    }
}
