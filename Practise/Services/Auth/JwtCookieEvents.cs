using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Practice.Services.Auth
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

        public static Task OnTokenValidated(TokenValidatedContext context)
        {
            var name = context.Principal?.Identity?.Name;
            Console.WriteLine($"JWT ok for: {name}");
            return Task.CompletedTask;
        }

        public static Task OnChallenge(JwtBearerChallengeContext context)
        {
            Console.WriteLine($"JWT challenge: {context.Error} | {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    }
}
