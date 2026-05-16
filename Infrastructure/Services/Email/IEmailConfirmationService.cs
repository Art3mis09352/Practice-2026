using Domain.Entities;

namespace Infrastructure.Services.Email
{
    public interface IEmailConfirmationService
    {
        Task SendConfirmationEmailAsync(User user);
        string DecodeToken(string encodedToken);
        string BuildResultRedirectUrl(bool succeeded, string message);
    }
}
