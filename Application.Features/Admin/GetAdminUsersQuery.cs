using Application.DTO.Admin;
using MediatR;

namespace Application.Features.Admin;

public sealed record GetAdminUsersQuery(GetAdminUsersQueryDTO Dto) : IRequest<PagedAdminUsersResponseDTO>;
