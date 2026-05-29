using Application.Data.DTO.Auth;
using Application.DTO.Auth;
using Application.Features.Common;
using Domain.Entities;
using Infrastructure.Services.Auth;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth;

public sealed class LoginHandler : IRequestHandler<LoginCommand, FeatureResult>
{
    private readonly UserManager<User> _userManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly AuthCookieService _authCookieService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginHandler(
        UserManager<User> userManager,
        JwtTokenService jwtTokenService,
        AuthCookieService authCookieService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _authCookieService = authCookieService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<FeatureResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return new FeatureUnauthorized(new AuthErrorResponseDTO
            {
                Code = "invalid_credentials",
                Message = "Неверный email или пароль."
            });
        }

        if (!user.EmailConfirmed)
        {
            return new FeatureForbiddenResult<AuthErrorResponseDTO>(new AuthErrorResponseDTO
            {
                Code = "email_not_confirmed",
                Message = "Подтвердите email перед входом."
            });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = await _jwtTokenService.GenerateTokenAsync(user);

        var httpResponse = _httpContextAccessor.HttpContext?.Response;
        if (httpResponse != null)
        {
            _authCookieService.SetAuthCookie(httpResponse, token);
        }

        var response = new ResponseLoginDTO
        {
            Email = user.Email ?? string.Empty,
            Username = user.UserName ?? string.Empty,
            Phone = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            Role = roles
        };

        return new FeatureOkResult<ResponseLoginDTO>(response);
    }
}
