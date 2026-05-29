using Application.Data.DTO.Route.Read;
using Application.Features.Common;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Unauthorized;

public sealed class GetPublicRouteHandler : IRequestHandler<GetPublicRouteQuery, FeatureResult>
{
    private readonly AppDbContext _appDbContext;

    public GetPublicRouteHandler(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<FeatureResult> Handle(GetPublicRouteQuery request, CancellationToken cancellationToken)
    {
        var route = await _appDbContext.Routes
            .AsNoTracking()
            .Where(x => x.Id == request.Id && x.IsPublic)
            .Include(x => x.Days.OrderBy(d => d.DayNumber))
                .ThenInclude(x => x.RouteDayBlocks.OrderBy(rdb => rdb.OrderInDay))
                    .ThenInclude(x => x.Block)
            .FirstOrDefaultAsync(cancellationToken);

        if (route == null)
        {
            return new FeatureNotFound();
        }

        var result = new RouteResponseDTO
        {
            Id = route.Id,
            Title = route.Title,
            Description = route.Description,
            CoverEmoji = route.CoverEmoji,
            StartDate = route.StartDate,
            EndDate = route.EndDate,
            IsPublic = route.IsPublic,
            ShareToken = route.ShareToken,
            Days = route.Days
                .OrderBy(d => d.DayNumber)
                .Select(day => new RouteDayInfoDTO
                {
                    Id = day.Id,
                    DayNumber = day.DayNumber,
                    Title = day.Title,
                    Notes = day.Notes,
                    Blocks = day.RouteDayBlocks
                        .OrderBy(rdb => rdb.OrderInDay)
                        .Where(rdb => rdb.Block != null)
                        .Select(rdb => new RouteDayBlockInfoDTO
                        {
                            Id = rdb.Id,
                            BlockId = rdb.BlockId,
                            OrderInDay = rdb.OrderInDay,
                            Notes = rdb.Notes,
                            Title = rdb.Block!.Title,
                            Category = rdb.Block.Category,
                            City = rdb.Block.City,
                            Address = rdb.Block.Address,
                            Latitude = rdb.Block.Latitude,
                            Longitude = rdb.Block.Longitude,
                            AvgPrice = rdb.Block.AvgPrice
                        })
                        .ToList()
                })
                .ToList(),
            Budget = route.Budget
        };

        return new FeatureOkResult<RouteResponseDTO>(result);
    }
}
