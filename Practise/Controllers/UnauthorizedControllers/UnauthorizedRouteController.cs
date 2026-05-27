using Application.Data.DTO.Route.Read;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Practice.Controllers.UnauthorizedControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnauthorizedRouteController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public UnauthorizedRouteController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Получение информации о маршруте",
            Description = "Возвращает информацию о публичном маршруте на основе его идентификатора."
        )]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> GetRouteInfo(int id)
        {
            var route = await _appDbContext.Routes
                .AsNoTracking()
                .Where(x => x.Id == id && x.IsPublic)
                .Include(x => x.User)
                .Include(x => x.Days.OrderBy(d => d.DayNumber))
                    .ThenInclude(x => x.RouteDayBlocks.OrderBy(rdb => rdb.OrderInDay))
                        .ThenInclude(x => x.Block)
                .FirstOrDefaultAsync();

            if (route == null)
            {
                return NotFound();
            }

            

            return Ok(MapToResponse(route));
        }

        [HttpGet("get routes info")]
        [SwaggerOperation(
            Summary = "Получение списка публичных маршрутов",
            Description = "Возвращает список публичных маршрутов с пагинацией."
        )]
        [ProducesResponseType(typeof(PagedRoutesResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedRoutesResponseDTO>> GetRoutes([FromQuery] GetRoutesQueryDTO queryDto)
        {
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

            var totalCount = await query.CountAsync();

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
                .ToListAsync();

            var response = new PagedRoutesResponseDTO
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(response);
        }

        [HttpGet("by-token/{token}")]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> GetByShareToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return NotFound();
            }

            var now = DateTime.UtcNow;

            var route = await _appDbContext.Routes
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Days.OrderBy(d => d.DayNumber))
                    .ThenInclude(x => x.RouteDayBlocks.OrderBy(rdb => rdb.OrderInDay))
                        .ThenInclude(x => x.Block)
                .FirstOrDefaultAsync(x =>
                    x.ShareLinks.Any(sl =>
                        sl.Token == token &&
                        sl.IsActive &&
                        sl.RevokedAt == null &&
                        sl.ExpiresAt > now));

            if (route == null)
            {
                return NotFound();
            }

            return Ok(MapToResponse(route));
        }

        private static RouteResponseDTO MapToResponse(Domain.Entities.Route route)
        {
            return new RouteResponseDTO
            {
                Id = route.Id,
                Title = route.Title,
                Description = route.Description,
                CoverEmoji = route.CoverEmoji,
                StartDate = route.StartDate,
                EndDate = route.EndDate,
                IsPublic = route.IsPublic,
                OwnerUsername = route.User?.UserName,
                LikesCount = route.LikesCount,
                IsLikedByCurrentUser = false,
                Budget = route.Budget,
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
                                Description = rdb.Block.Description,
                                Category = rdb.Block.Category,
                                City = rdb.Block.City,
                                Address = rdb.Block.Address,
                                Latitude = rdb.Block.Latitude,
                                Longitude = rdb.Block.Longitude,
                                AvgPrice = rdb.Block.AvgPrice
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }


    }
}
