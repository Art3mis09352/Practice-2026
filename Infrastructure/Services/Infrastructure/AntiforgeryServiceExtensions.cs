using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Infrastructure
{
    public static class AntiforgeryServiceExtensions
    {
        public const string AntiforgeryHeaderName = "X-XSRF-TOKEN";
        public const string AntiforgeryCookieName = "XSRF-TOKEN";

        public static IServiceCollection AddAppAntiforgery(this IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.Name = "Antiforgery";
                options.Cookie.HttpOnly = false;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            return services;
        }

        public static IApplicationBuilder UseAppAntiforgery(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var method = context.Request.Method;

                if (!HttpMethods.IsPost(method) &&
                    !HttpMethods.IsPut(method) &&
                    !HttpMethods.IsPatch(method) &&
                    !HttpMethods.IsDelete(method))
                {
                    await next();
                    return;
                }

                var path = context.Request.Path.Value ?? string.Empty;

                var ignoredPaths = new[]
                {
                    "/api/Auth/login",
                    "/api/Auth/register",
                    "/api/Auth/register-owner",
                    "/api/Auth/antiforgery"
                };

                if (ignoredPaths.Any(x => path.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
                {
                    await next();
                    return;
                }

                var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();

                try
                {
                    await antiforgery.ValidateRequestAsync(context);
                }
                catch
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("CSRF token validation failed.");
                    return;
                }

                await next();
            });
        }
    }
}
