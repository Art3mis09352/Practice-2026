using Application.Features.Common;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users;

public sealed class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, FeatureResult>
{
    private readonly UserManager<User> _userManager;

    public UpdateUserProfileHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<FeatureResult> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
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
        if (dto.Username != null)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                return new FeatureBadRequest("Username не может быть пустым.");
            }

            user.UserName = dto.Username.Trim();
        }

        if (dto.Phone != null)
        {
            user.PhoneNumber = string.IsNullOrWhiteSpace(dto.Phone)
                ? null
                : dto.Phone.Trim();
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return new FeatureBadRequest(result.Errors.Select(x => x.Description));
        }

        return new FeatureNoContent();
    }
}
