using Application.DTO.Block;
using MediatR;

namespace Application.Features.Blocks;

public sealed record GetApprovedBlocksQuery(GetBlocksQueryDTO Dto) : IRequest<PagedBlocksResponseDTO>;
