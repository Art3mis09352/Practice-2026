using Application.Features.Common;
using Infrastructure.Services.Auth;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Auth;

public sealed class LogoutHandler : IRequestHandler<LogoutCommand, FeatureResult>
{
    private readonly AuthCookieService _authCookieService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogoutHandler(AuthCookieService authCookieService, IHttpContextAccessor httpContextAccessor)
    {
        _authCookieService = authCookieService;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<FeatureResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        if (response != null)
        {
            _authCookieService.ClearAuthCookie(response);
        }

        return Task.FromResult<FeatureResult>(new FeatureNoContent());
    }
}
