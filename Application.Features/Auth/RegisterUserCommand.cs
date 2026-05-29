using Application.Data.DTO.Auth;
using Application.Features.Common;
using MediatR;

namespace Application.Features.Auth;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string? Phone,
    string? Username,
    string Role,
    string RegisterActionName) : IRequest<FeatureResult>;
