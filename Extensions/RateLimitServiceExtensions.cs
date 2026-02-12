using Base.Api.Config;
using System.Threading.RateLimiting;

namespace Base.Api.Extensions
{
    public static class RateLimitServiceExtensions
    {
        public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration config)
        {
            var loginOptions = config.GetSection("RateLimiting:LoginPolicy").Get<RateLimitPolicyOptions>();

            services.AddRateLimiter(options =>
            {
                options.AddPolicy("LoginPolicy", context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.Get(
                        ip,
                        key => new FixedWindowRateLimiter(
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = loginOptions.PermitLimit,
                                Window = TimeSpan.FromMinutes(loginOptions.WindowMinutes),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = loginOptions.QueueLimit
                            }
                        )
                    );
                });
            });

            return services;
        }

        public static WebApplication UseRateLimiting(this WebApplication app)
        {
            app.UseRateLimiter(); // globally applies configured policies
            return app;
        }
    }
}
