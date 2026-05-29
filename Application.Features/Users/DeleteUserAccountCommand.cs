using Application.Features.Common;
using MediatR;

namespace Application.Features.Users;

public sealed record DeleteUserAccountCommand(string? UserId) : IRequest<FeatureResult>;
