using Microsoft.OpenApi;

namespace Base.Api.Extensions
{
    public static class SwaggerServiceExtensions
    {
        // Register Swagger services
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                // JWT Bearer configuration
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Enter JWT token (no 'Bearer ' prefix)"
                });
            });

            return services;
        }

        // Configure Swagger middleware
        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basi.Api.v1");
                c.RoutePrefix = ""; // serve at root "/"

                // UI settings
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // collapse endpoints
                c.DefaultModelsExpandDepth(-1); // hide schemas/models section
                c.DefaultModelExpandDepth(0); // collapse models
                c.EnableFilter(); // search box
                c.DisplayRequestDuration(); // show request duration
            });

            return app;
        }
    }
}
