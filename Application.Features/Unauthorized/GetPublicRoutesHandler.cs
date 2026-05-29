using Application.Data.DTO.Route.Read;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Unauthorized;

public sealed class GetPublicRoutesHandler : IRequestHandler<GetPublicRoutesQuery, PagedRoutesResponseDTO>
{
    private readonly AppDbContext _appDbContext;

    public GetPublicRoutesHandler(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<PagedRoutesResponseDTO> Handle(GetPublicRoutesQuery request, CancellationToken cancellationToken)
    {
        var queryDto = request.Dto;
        var page = queryDto.Page < 1 ? 1 : queryDto.Page;
        var pageSize = queryDto.PageSize < 1 ? 10 : queryDto.PageSize;
        if (pageSize > 50) pageSize = 50;

        var query = _appDbContext.Routes
            .AsNoTracking()
            .Where(x => x.IsPublic)
            .Include(x => x.Days)
                .ThenInclude(x => x.RouteDayBlocks)
                    .ThenInclude(x => x.Block)
            .AsQueryable();

        if (queryDto.StartDateFrom.HasValue)
        {
            query = query.Where(x => x.StartDate >= queryDto.StartDateFrom.Value);
        }

        if (queryDto.StartDateTo.HasValue)
        {
            query = query.Where(x => x.StartDate <= queryDto.StartDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.City))
        {
            query = query.Where(x =>
                x.Days.Any(d =>
                    d.RouteDayBlocks.Any(rdb =>
                        rdb.Block != null && rdb.Block.City == queryDto.City)));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Search))
        {
            var search = queryDto.Search.Trim().ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var popularSort = string.Equals(
            queryDto.SortBy?.Trim(),
            "popular",
            StringComparison.OrdinalIgnoreCase);

        var sortedQuery = popularSort
            ? query.OrderByDescending(x => x.LikesCount)
                .ThenBy(x => x.StartDate)
                .ThenBy(x => x.Title)
            : query.OrderBy(x => x.StartDate)
                .ThenBy(x => x.Title);

        var items = await sortedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(route => new RoutePreviewDTO
            {
                Id = route.Id,
                Title = route.Title,
                Description = route.Description,
                CoverEmoji = route.CoverEmoji,
                StartDate = route.StartDate,
                EndDate = route.EndDate,
                DaysCount = route.Days.Count,
                PointsCount = route.Days.SelectMany(d => d.RouteDayBlocks).Count(),
                FirstCity = route.Days
                    .SelectMany(d => d.RouteDayBlocks)
                    .Where(rdb => rdb.Block != null)
                    .Select(rdb => rdb.Block!.City)
                    .FirstOrDefault(),
                Budget = route.Budget,
                IsPublic = route.IsPublic,
                OwnerUsername = route.User != null ? route.User.UserName : null,
                LikesCount = route.LikesCount,
                IsLikedByCurrentUser = false
            })
            .ToListAsync(cancellationToken);

        return new PagedRoutesResponseDTO
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
