using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.Auth
{
    public class AuthCookieService
    {
        public const string CookieName = "auth";

        public void SetAuthCookie(HttpResponse response, string jwt)
        {
            response.Cookies.Append(CookieName, jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddHours(12),
                Path = "/"
            });
        }

        public void ClearAuthCookie(HttpResponse response)
        {
            response.Cookies.Delete(CookieName, new CookieOptions
            {
                HttpOnly = true,          
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });
        }

    }
}
