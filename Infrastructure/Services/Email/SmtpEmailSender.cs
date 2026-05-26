using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Services.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailSender> _logger;
        private readonly IWebHostEnvironment _environment;

        public SmtpEmailSender(
            IConfiguration configuration,
            ILogger<SmtpEmailSender> logger,
            IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _logger = logger;
            _environment = environment;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var host = _configuration["Email:Smtp:Host"];
            var portText = _configuration["Email:Smtp:Port"];
            var username = _configuration["Email:Smtp:Username"];
            var password = _configuration["Email:Smtp:Password"];
            var fromEmail = _configuration["Email:Smtp:FromEmail"];
            var fromName = _configuration["Email:Smtp:FromName"];
            var enableSslText = _configuration["Email:Smtp:EnableSsl"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(fromEmail) ||
                string.IsNullOrWhiteSpace(portText))
            {
                if (_environment.IsDevelopment() || _environment.IsEnvironment("Testing"))
                {
                    _logger.LogInformation(
                        "Email config is missing. Skipping send in {Environment}. To={ToEmail}, Subject={Subject}, Body={Body}",
                        _environment.EnvironmentName,
                        toEmail,
                        subject,
                        htmlBody);
                    return;
                }

                throw new InvalidOperationException("SMTP settings are not configured.");
            }

            if (!int.TryParse(portText, out var port))
            {
                throw new InvalidOperationException("SMTP port is invalid.");
            }

            var enableSsl = !string.IsNullOrWhiteSpace(enableSslText) &&
                bool.TryParse(enableSslText, out var parsedEnableSsl) &&
                parsedEnableSsl;

            using var message = new MailMessage
            {
                From = string.IsNullOrWhiteSpace(fromName)
                    ? new MailAddress(fromEmail)
                    : new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrWhiteSpace(username))
            {
                client.Credentials = new NetworkCredential(username, password);
            }
            Console.WriteLine($"Host: {host}");
            Console.WriteLine($"Port: {port}");
            Console.WriteLine($"Username: {username}");
            Console.WriteLine($"Password length: {password?.Length}");
            Console.WriteLine($"FromEmail: {fromEmail}");
            Console.WriteLine($"EnableSsl: {enableSsl}");

            await client.SendMailAsync(message);
        }
    }
}
