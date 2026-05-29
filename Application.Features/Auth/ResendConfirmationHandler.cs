using Application.Data.DTO.Auth;
using Domain.Entities;
using Infrastructure.Services.Email;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth;

public sealed class ResendConfirmationHandler : IRequestHandler<ResendConfirmationCommand, ConfirmEmailResultDTO>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailConfirmationService _emailConfirmationService;

    public ResendConfirmationHandler(
        UserManager<User> userManager,
        IEmailConfirmationService emailConfirmationService)
    {
        _userManager = userManager;
        _emailConfirmationService = emailConfirmationService;
    }

    public async Task<ConfirmEmailResultDTO> Handle(ResendConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Dto.Email);
        if (user != null && !user.EmailConfirmed)
        {
            await _emailConfirmationService.SendConfirmationEmailAsync(user);
        }

        return new ConfirmEmailResultDTO
        {
            Succeeded = true,
            Message = "Если аккаунт существует и email еще не подтвержден, письмо будет отправлено повторно."
        };
    }
}
