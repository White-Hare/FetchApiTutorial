using FetchApiTutorial.Helpers;
using FetchApiTutorial.Helpers.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FetchApiTutorial.Installers
{
    public class SettingsInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<DatabaseSettings>(configuration.GetSection("MongoDatabaseSettings"));
        }
    }
}
