using Application.DTO.Block;
using Application.Features.Common;
using MediatR;

namespace Application.Features.Owner;

public sealed record UpdateOwnerBlockCommand(int Id, UpdateBlockRequestDTO Dto, string? OwnerId) : IRequest<FeatureResult>;
