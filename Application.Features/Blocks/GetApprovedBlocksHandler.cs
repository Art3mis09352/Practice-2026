using Application.DTO.Block;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Blocks;

public sealed class GetApprovedBlocksHandler : IRequestHandler<GetApprovedBlocksQuery, PagedBlocksResponseDTO>
{
    private readonly AppDbContext _dbContext;

    public GetApprovedBlocksHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedBlocksResponseDTO> Handle(GetApprovedBlocksQuery request, CancellationToken cancellationToken)
    {
        var queryDto = request.Dto;
        var page = queryDto.Page < 1 ? 1 : queryDto.Page;
        var pageSize = queryDto.PageSize < 1 ? 10 : queryDto.PageSize;
        if (pageSize > 50) pageSize = 50;

        var query = _dbContext.Blocks
            .AsNoTracking()
            .Where(b => b.IsApproved)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryDto.City))
        {
            query = query.Where(b => b.City == queryDto.City);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Search))
        {
            var search = queryDto.Search.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(search) ||
                (b.Address != null && b.Address.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Category))
        {
            query = query.Where(b => b.Category == queryDto.Category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BlockPreviewDTO
            {
                Id = b.Id,
                Title = b.Title,
                Category = b.Category,
                City = b.City,
                Address = b.Address,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                IsApproved = b.IsApproved
            })
            .ToListAsync(cancellationToken);

        return new PagedBlocksResponseDTO
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
