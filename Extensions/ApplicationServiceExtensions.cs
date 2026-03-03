using Base.Api.Data;
using Base.Api.Helpers;
using Base.Api.Repositories;
using Base.Api.Repositories.Interfaces;
using Base.Api.Services;
using Base.Api.Services.Interfaces;

namespace Base.Api.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // Services
            services.AddScoped<IAuthService, AuthServices>();
            services.AddScoped<IUserService, UserServices>();

            // Helpers
            services.AddSingleton<JwtHelper>();
            services.AddSingleton<Connection>();

            return services;
        }
    }
}
