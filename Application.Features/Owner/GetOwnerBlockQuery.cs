using Application.DTO.Block;
using Application.Features.Common;
using MediatR;

namespace Application.Features.Owner;

public sealed record GetOwnerBlockQuery(int Id, string? OwnerId) : IRequest<FeatureResult>;
