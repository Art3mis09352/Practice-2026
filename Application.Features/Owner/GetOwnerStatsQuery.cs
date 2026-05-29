using Application.Features.Common;
using MediatR;

namespace Application.Features.Owner;

public sealed record GetOwnerStatsQuery(string? OwnerId, bool IsOwnerRole) : IRequest<FeatureResult>;
