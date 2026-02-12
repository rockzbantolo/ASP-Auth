using Base.Api.Helpers;
using Base.Api.Repositories;
using Base.Api.Repositories.Interfaces;
using Base.Api.Services;
using Base.Api.Services.Interfaces;

namespace Base.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IUserRepo, UserRepo>();

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();

            // Helpers
            services.AddScoped<JwtHelper>();

            return services;
        }
    }
}
