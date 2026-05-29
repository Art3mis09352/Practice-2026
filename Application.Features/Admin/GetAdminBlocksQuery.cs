using Application.DTO.Admin;
using Application.DTO.Block;
using MediatR;

namespace Application.Features.Admin;

public sealed record GetAdminBlocksQuery(GetAdminBlocksQueryDTO Dto, bool PendingOnly) : IRequest<PagedBlocksResponseDTO>;
