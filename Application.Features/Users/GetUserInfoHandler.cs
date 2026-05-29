using Application.DTO.User;
using Application.Features.Common;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users;

public sealed class GetUserInfoHandler : IRequestHandler<GetUserInfoQuery, FeatureResult>
{
    private readonly UserManager<User> _userManager;

    public GetUserInfoHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<FeatureResult> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
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

        var roles = await _userManager.GetRolesAsync(user);
        var response = new UserInfoResponseDTO
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Username = user.UserName ?? string.Empty,
            Phone = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            IsConfirmed = user.EmailConfirmed,
            Roles = roles
        };

        return new FeatureOkResult<UserInfoResponseDTO>(response);
    }
}
