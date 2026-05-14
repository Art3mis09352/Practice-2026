using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using User = Domain.Entities.User;

namespace Infrastructure.Services.Auth
{
    public static class JwtCookieEvents
    {
        public static Task OnMessageReceived(MessageReceivedContext context)
        {
            if (context.Request.Cookies.TryGetValue(AuthCookieService.CookieName, out var token))
            {
                context.Token = token;
                return Task.CompletedTask;
            }

            var authHeader = context.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
            {
                context.Token = authHeader["Bearer ".Length..].Trim();
            }

            return Task.CompletedTask;
        }

        public static Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            Console.WriteLine($"JWT auth failed: {context.Exception.Message}");
            return Task.CompletedTask;
        }

        public static async Task OnTokenValidated(TokenValidatedContext context)
        {
            var userManager = context.HttpContext.RequestServices.GetService(typeof(UserManager<User>)) as UserManager<User>;
            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userManager == null || string.IsNullOrWhiteSpace(userId))
            {
                context.Fail("JWT user validation failed.");
                return;
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                context.Fail("Пользователь больше не существует.");
                return;
            }

            var tokenSecurityStamp = context.Principal?.FindFirst("security_stamp")?.Value;
            if (!string.IsNullOrWhiteSpace(tokenSecurityStamp) &&
                !string.Equals(tokenSecurityStamp, user.SecurityStamp, StringComparison.Ordinal))
            {
                context.Fail("JWT security stamp is outdated.");
                return;
            }

            var name = context.Principal?.Identity?.Name;
            Console.WriteLine($"JWT ok for: {name}");
        }

        public static Task OnChallenge(JwtBearerChallengeContext context)
        {
            Console.WriteLine($"JWT challenge: {context.Error} | {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    }
}
