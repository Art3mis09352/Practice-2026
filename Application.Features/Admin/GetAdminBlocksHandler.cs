using Application.DTO.Block;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin;

public sealed class GetAdminBlocksHandler : IRequestHandler<GetAdminBlocksQuery, PagedBlocksResponseDTO>
{
    private readonly AppDbContext _dbContext;

    public GetAdminBlocksHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedBlocksResponseDTO> Handle(GetAdminBlocksQuery request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var pendingOnly = request.PendingOnly;
        var page = dto.Page < 1 ? 1 : dto.Page;
        var pageSize = dto.PageSize < 1 ? 10 : dto.PageSize;
        if (pageSize > 50) pageSize = 50;

        var query = _dbContext.Blocks
            .AsNoTracking()
            .AsQueryable();

        if (pendingOnly)
        {
            query = query.Where(b => !b.IsApproved);
        }

        if (!string.IsNullOrWhiteSpace(dto.Search))
        {
            var search = dto.Search.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(search) ||
                (b.Address != null && b.Address.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(b => b.IsApproved)
            .ThenBy(b => b.Title)
            .ThenBy(b => b.Id)
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
