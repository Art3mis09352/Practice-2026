using Application.DTO.Block;
using Application.Features.Common;
using MediatR;

namespace Application.Features.Unauthorized;

public sealed record GetPublicPointQuery(int Id) : IRequest<FeatureResult>;
