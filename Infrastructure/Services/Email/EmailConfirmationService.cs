using Domain.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Infrastructure.Services.Email
{
    public class EmailConfirmationService : IEmailConfirmationService
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public EmailConfirmationService(
            UserManager<User> userManager,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task SendConfirmationEmailAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("User email is required for confirmation.");
            }

            var publicApiBaseUrl = _configuration["EmailConfirmation:PublicApiBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(publicApiBaseUrl))
            {
                throw new InvalidOperationException("Email confirmation public API base URL is not configured.");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var confirmationUrl = QueryHelpers.AddQueryString(
                $"{publicApiBaseUrl}/api/auth/confirm-email",
                new Dictionary<string, string?>
                {
                    ["userId"] = user.Id,
                    ["token"] = encodedToken
                });

            var body = $"""
                <p>Подтвердите вашу почту, чтобы завершить регистрацию.</p>
                <p><a href="{confirmationUrl}">Подтвердить email</a></p>
                <p>Если вы не регистрировались, просто проигнорируйте это письмо.</p>
                """;

            await _emailSender.SendEmailAsync(user.Email, "Подтверждение email", body);
        }

        public string DecodeToken(string encodedToken)
        {
            var bytes = WebEncoders.Base64UrlDecode(encodedToken);
            return Encoding.UTF8.GetString(bytes);
        }

        public string BuildResultRedirectUrl(bool succeeded, string message)
        {
            var frontendResultUrl = _configuration["EmailConfirmation:FrontendResultUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(frontendResultUrl))
            {
                return string.Empty;
            }

            return QueryHelpers.AddQueryString(
                frontendResultUrl,
                new Dictionary<string, string?>
                {
                    ["status"] = succeeded ? "success" : "error",
                    ["message"] = message
                });
        }
    }
}
