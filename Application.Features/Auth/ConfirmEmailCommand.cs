using Application.Features.Common;
using MediatR;

namespace Application.Features.Auth;

public sealed record ConfirmEmailCommand(string? UserId, string? Token) : IRequest<FeatureResult>;
