using Application.Data.DTO.Auth;
using Application.Features.Common;
using Domain.Entities;
using Infrastructure.Services.Email;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth;

public sealed class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, FeatureResult>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailConfirmationService _emailConfirmationService;

    public ConfirmEmailHandler(
        UserManager<User> userManager,
        IEmailConfirmationService emailConfirmationService)
    {
        _userManager = userManager;
        _emailConfirmationService = emailConfirmationService;
    }

    public async Task<FeatureResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Token))
        {
            return BuildConfirmEmailResponse(false, "Некорректная ссылка подтверждения.");
        }

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return BuildConfirmEmailResponse(false, "Пользователь не найден.");
        }

        if (user.EmailConfirmed)
        {
            return BuildConfirmEmailResponse(true, "Email уже подтвержден.");
        }

        string decodedToken;
        try
        {
            decodedToken = _emailConfirmationService.DecodeToken(request.Token);
        }
        catch
        {
            return BuildConfirmEmailResponse(false, "Токен подтверждения недействителен.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(x => x.Description));
            return BuildConfirmEmailResponse(false, string.IsNullOrWhiteSpace(message)
                ? "Не удалось подтвердить email."
                : message);
        }

        return BuildConfirmEmailResponse(true, "Email успешно подтвержден.");
    }

    private FeatureResult BuildConfirmEmailResponse(bool succeeded, string message)
    {
        var redirectUrl = _emailConfirmationService.BuildResultRedirectUrl(succeeded, message);
        if (!string.IsNullOrWhiteSpace(redirectUrl))
        {
            return new FeatureRedirect(redirectUrl);
        }

        if (succeeded)
        {
            return new FeatureOkResult<ConfirmEmailResultDTO>(new ConfirmEmailResultDTO
            {
                Succeeded = true,
                Message = message
            });
        }

        return new FeatureBadRequest(new ConfirmEmailResultDTO
        {
            Succeeded = false,
            Message = message
        });
    }
}
