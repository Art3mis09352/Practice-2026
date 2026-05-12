using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Infrastructure
{
    public static class CorsServiceExtensions
    {
        public static IServiceCollection AddFrontendCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("FrontendCorsPolicy", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:5173",
                            "http://localhost:4173",
                            "https://localhost:5173",
                            "https://localhost:4173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            return services;
        }
    }
}
