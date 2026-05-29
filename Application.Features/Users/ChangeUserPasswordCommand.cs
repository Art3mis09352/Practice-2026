using Application.DTO.User;
using Application.Features.Common;
using MediatR;

namespace Application.Features.Users;

public sealed record ChangeUserPasswordCommand(string? UserId, ChangePasswordDTO Dto) : IRequest<FeatureResult>;
