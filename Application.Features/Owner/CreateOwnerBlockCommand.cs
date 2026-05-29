using Application.DTO.Block;
using Application.Features.Common;
using MediatR;

namespace Application.Features.Owner;

public sealed record CreateOwnerBlockCommand(CreateBlockRequestDTO Dto, string? OwnerId) : IRequest<FeatureResult>;
