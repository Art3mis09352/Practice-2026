using Application.Features.Common;
using MediatR;

namespace Application.Features.Auth;

public sealed record LogoutCommand : IRequest<FeatureResult>;
