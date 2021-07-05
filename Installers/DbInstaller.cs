using FetchApiTutorial.Data;
using FetchApiTutorial.Services.MyTaskService;
using FetchApiTutorial.Services.UserService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FetchApiTutorial.Installers
{
    public class DbInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IDatabase, MongoDatabase>();
            services.AddSingleton<IMyTaskService, MongoDbMyTaskService>();
            services.AddSingleton<IUserService, UserService>();
        }
    }
}
