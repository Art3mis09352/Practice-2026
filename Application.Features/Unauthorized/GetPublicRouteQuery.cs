using Application.Data.DTO.Route.Read;
using Application.Features.Common;
using MediatR;

namespace Application.Features.Unauthorized;

public sealed record GetPublicRouteQuery(int Id) : IRequest<FeatureResult>;
