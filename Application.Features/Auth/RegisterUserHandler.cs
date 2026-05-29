using Application.Data.DTO.Auth;
using Application.Features.Common;
using Domain.Entities;
using Infrastructure.Services.Email;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, FeatureResult>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailConfirmationService _emailConfirmationService;

    public RegisterUserHandler(
        UserManager<User> userManager,
        IEmailConfirmationService emailConfirmationService)
    {
        _userManager = userManager;
        _emailConfirmationService = emailConfirmationService;
    }

    public async Task<FeatureResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new FeatureConflict("Пользователь с таким email уже существует.");
        }

        var user = new User
        {
            Email = request.Email,
            UserName = string.IsNullOrWhiteSpace(request.Username) ? request.Email : request.Username,
            PhoneNumber = request.Phone
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return new FeatureBadRequest(createResult.Errors.Select(e => e.Description));
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!addRoleResult.Succeeded)
        {
            return new FeatureBadRequest(addRoleResult.Errors.Select(x => x.Description));
        }

        await _emailConfirmationService.SendConfirmationEmailAsync(user);

        return new FeatureCreatedResult<ResponseRegisterDTO>(
            new ResponseRegisterDTO
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                Phone = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                RequiresEmailConfirmation = true,
                Message = "Аккаунт создан. Подтвердите email перед входом.",
                Roles = new List<string> { request.Role }
            },
            request.RegisterActionName,
            new { id = user.Id });
    }
}
