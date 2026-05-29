using Application.Features.Common;
using Domain.Entities;
using Infrastructure.Services.Auth;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users;

public sealed class ChangeUserPasswordHandler : IRequestHandler<ChangeUserPasswordCommand, FeatureResult>
{
    private readonly UserManager<User> _userManager;
    private readonly AuthCookieService _authCookieService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChangeUserPasswordHandler(
        UserManager<User> userManager,
        AuthCookieService authCookieService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _authCookieService = authCookieService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<FeatureResult> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            return new FeatureUnauthorized();
        }

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new FeatureNotFound("Пользователь не найден.");
        }

        var dto = request.Dto;
        if (dto.CurrentPassword == dto.NewPassword)
        {
            return new FeatureBadRequest("Новый пароль должен отличаться от текущего.");
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            return new FeatureBadRequest(result.Errors.Select(x => x.Description));
        }

        var response = _httpContextAccessor.HttpContext?.Response;
        if (response != null)
        {
            _authCookieService.ClearAuthCookie(response);
        }

        return new FeatureNoContent();
    }
}
