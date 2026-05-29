using Application.Features.Common;
using Domain.Entities;
using Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin;

public sealed class DeleteAdminUserHandler : IRequestHandler<DeleteAdminUserCommand, FeatureResult>
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _dbContext;

    public DeleteAdminUserHandler(UserManager<User> userManager, AppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<FeatureResult> Handle(DeleteAdminUserCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.CurrentUserId) && request.CurrentUserId == request.Id)
        {
            return new FeatureBadRequest("Администратор не может удалить сам себя.");
        }

        var user = await _userManager.FindByIdAsync(request.Id);
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
