using Application.Data.DTO.Auth;
using MediatR;

namespace Application.Features.Auth;

public sealed record ResendConfirmationCommand(ResendConfirmationDTO Dto) : IRequest<ConfirmEmailResultDTO>;
