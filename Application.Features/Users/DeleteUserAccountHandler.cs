using Application.Features.Common;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services.Auth;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users;

public sealed class DeleteUserAccountHandler : IRequestHandler<DeleteUserAccountCommand, FeatureResult>
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly AuthCookieService _authCookieService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeleteUserAccountHandler(
        UserManager<User> userManager,
        AppDbContext dbContext,
        AuthCookieService authCookieService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _authCookieService = authCookieService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<FeatureResult> Handle(DeleteUserAccountCommand request, CancellationToken cancellationToken)
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

        await DeleteUserDependenciesAsync(user.Id, cancellationToken);

        var result = await _userManager.DeleteAsync(user);
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

    private async Task DeleteUserDependenciesAsync(string userId, CancellationToken cancellationToken)
    {
        var routeLikes = await _dbContext.RouteLikes
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var routes = await _dbContext.Routes
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var blocks = await _dbContext.Blocks
            .Where(x => x.OwnerId == userId)
            .ToListAsync(cancellationToken);

        if (routeLikes.Count > 0)
        {
            _dbContext.RouteLikes.RemoveRange(routeLikes);
        }

        if (routes.Count > 0)
        {
            _dbContext.Routes.RemoveRange(routes);
        }

        if (blocks.Count > 0)
        {
            _dbContext.Blocks.RemoveRange(blocks);
        }

        if (routeLikes.Count > 0 || routes.Count > 0 || blocks.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
