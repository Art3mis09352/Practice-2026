using Application.Data.DTO.Route.Read;
using MediatR;

namespace Application.Features.Unauthorized;

public sealed record GetPublicRoutesQuery(GetRoutesQueryDTO Dto) : IRequest<PagedRoutesResponseDTO>;
