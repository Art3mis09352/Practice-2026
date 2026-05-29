using Application.DTO.Block;
using Application.Features.Common;
using MediatR;

namespace Application.Features.Owner;

public sealed record GetOwnerPointsQuery(GetBlocksQueryDTO Dto, string? OwnerId, bool IsOwnerRole) : IRequest<FeatureResult>;
