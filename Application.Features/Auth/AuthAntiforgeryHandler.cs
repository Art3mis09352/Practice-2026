using Application.Features.Common;
using Infrastructure.Services.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Auth;

public sealed class AuthAntiforgeryHandler : IRequestHandler<AuthAntiforgeryCommand, FeatureResult>
{
    private readonly IAntiforgery _antiforgery;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthAntiforgeryHandler(IAntiforgery antiforgery, IHttpContextAccessor httpContextAccessor)
    {
        _antiforgery = antiforgery;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<FeatureResult> Handle(AuthAntiforgeryCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        var tokens = _antiforgery.GetAndStoreTokens(httpContext);

        httpContext.Response.Cookies.Append(
            AntiforgeryServiceExtensions.AntiforgeryRequestTokenCookieName,
            tokens.RequestToken!,
            new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

        return Task.FromResult<FeatureResult>(new FeatureNoContent());
    }
}
