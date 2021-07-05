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
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.Configure<DatabaseSettings>(configuration.GetSection("MongoDatabaseSettings"));
        }
    }
}
