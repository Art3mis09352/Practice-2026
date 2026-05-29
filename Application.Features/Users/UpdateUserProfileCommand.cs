using Application.DTO.User;
using Application.Features.Common;
using MediatR;

namespace Application.Features.Users;

public sealed record UpdateUserProfileCommand(string? UserId, UpdateUserProfileDTO Dto) : IRequest<FeatureResult>;
