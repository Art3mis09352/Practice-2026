using Application.Data.DTO.Route.Read;
using Application.Data.DTO.Route.Request;
using Application.DTO.Route.Create;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Route = Domain.Entities.Route;

namespace Infrastructure.Services.Users
{
    public class UserRouteService : IUserRouteService
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public UserRouteService(AppDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        

        public async Task<RouteResponseDTO> CreateRouteAsync(string userId, CreateRouteDTO dto)
        {
            ValidateRouteDates(dto.StartDate, dto.EndDate);
            ValidateBudget(dto.Budget);

            if (dto.Days == null || dto.Days.Count == 0)
            {
                throw new InvalidOperationException("Маршрут должен содержать хотя бы один день.");
            }

            ValidateCreateDays(dto.Days, dto.StartDate, dto.EndDate);

            var requestedBlockIds = dto.Days
                .SelectMany(x => x.Blocks)
                .Select(x => x.BlockId)
                .Distinct()
                .ToList();

            await EnsureApprovedBlocksAsync(requestedBlockIds);

            var route = new Route
            {
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                CoverEmoji = NormalizeCoverEmoji(dto.CoverEmoji),
                Budget = dto.Budget,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsPublic = dto.IsPublic,
                LikesCount = 0,
                Days = dto.Days
                    .OrderBy(x => x.DayNumber)
                    .Select(dayDto => new RouteDay
                    {
                        DayNumber = dayDto.DayNumber,
                        Title = dayDto.Title,
                        Notes = dayDto.Notes,
                        RouteDayBlocks = dayDto.Blocks
                            .OrderBy(x => x.OrderInDay)
                            .Select(blockDto => new RouteDayBlock
                            {
                                BlockId = blockDto.BlockId,
                                OrderInDay = blockDto.OrderInDay,
                                Notes = blockDto.Notes
                            })
                            .ToList()
                    })
                    .ToList()
            };

            _dbContext.Routes.Add(route);
            await _dbContext.SaveChangesAsync();

            var createdRoute = await GetUserRouteWithDetailsAsync(userId, route.Id)
                ?? throw new InvalidOperationException("Не удалось загрузить созданный маршрут.");

            return MapRouteResponse(createdRoute, userId);
        }

        public async Task<RouteShareLinkResponseDTO?> GetShareLinkAsync(string userId, int routeId)
        {
            var routeExists = await _dbContext.Routes
                .AnyAsync(x => x.Id == routeId && x.UserId == userId);

            if (!routeExists)
            {
                return null;
            }

            var now = DateTime.UtcNow;

            var link = await _dbContext.RouteShareLinks
                .AsNoTracking()
                .Where(x => x.RouteId == routeId && x.IsActive && x.RevokedAt == null && x.ExpiresAt > now)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            return link == null ? null : MapShareLink(link);
        }
        public async Task<RouteShareLinkResponseDTO?> CreateShareLinkAsync(string userId, int routeId)
        {
            var route = await _dbContext.Routes
                .FirstOrDefaultAsync(x => x.Id == routeId && x.UserId == userId);

            if (route == null)
            {
                return null;
            }

            var now = DateTime.UtcNow;

            var activeLink = await _dbContext.RouteShareLinks
                .FirstOrDefaultAsync(x => x.RouteId == routeId && x.IsActive && x.RevokedAt == null && x.ExpiresAt > now);

            if (activeLink != null)
            {
                return MapShareLink(activeLink);
            }

            await DeactivateAllShareLinksAsync(routeId, now);

            var link = new RouteShareLink
            {
                RouteId = routeId,
                Token = GenerateShareToken(),
                IsActive = true,
                CreatedAt = now,
                ExpiresAt = now.AddDays(GetShareLifetimeDays())
            };

            _dbContext.RouteShareLinks.Add(link);
            await _dbContext.SaveChangesAsync();

            return MapShareLink(link);
        }

        public async Task<bool> RevokeShareLinkAsync(string userId, int routeId)
        {
            var routeExists = await _dbContext.Routes
                .AnyAsync(x => x.Id == routeId && x.UserId == userId);

            if (!routeExists)
            {
                return false;
            }

            var now = DateTime.UtcNow;

            var activeLinks = await _dbContext.RouteShareLinks
                .Where(x => x.RouteId == routeId && x.IsActive && x.RevokedAt == null)
                .ToListAsync();

            if (activeLinks.Count == 0)
            {
                return true;
            }

            foreach (var link in activeLinks)
            {
                link.IsActive = false;
                link.RevokedAt = now;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<RouteShareLinkResponseDTO?> RegenerateShareLinkAsync(string userId, int routeId)
        {
            var route = await _dbContext.Routes
                .FirstOrDefaultAsync(x => x.Id == routeId && x.UserId == userId);

            if (route == null)
            {
                return null;
            }

            var now = DateTime.UtcNow;

            await DeactivateAllShareLinksAsync(routeId, now);

            var link = new RouteShareLink
            {
                RouteId = routeId,
                Token = GenerateShareToken(),
                IsActive = true,
                CreatedAt = now,
                ExpiresAt = now.AddDays(GetShareLifetimeDays())
            };

            _dbContext.RouteShareLinks.Add(link);
            await _dbContext.SaveChangesAsync();

            return MapShareLink(link);
        }

        public async Task<PagedRoutesResponseDTO> GetMyRoutesAsync(string userId, GetRoutesQueryDTO dto)
        {
            var page = dto.Page < 1 ? 1 : dto.Page;
            var pageSize = dto.PageSize < 1 ? 10 : dto.PageSize;
            if (pageSize > 50) pageSize = 50;

            var query = _dbContext.Routes
                .Include(x => x.Likes)
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Include(x => x.Days)
                    .ThenInclude(x => x.RouteDayBlocks)
                        .ThenInclude(x => x.Block)
                .AsQueryable();

            if (dto.StartDateFrom.HasValue)
            {
                query = query.Where(x => x.StartDate >= dto.StartDateFrom.Value);
            }

            if (dto.StartDateTo.HasValue)
            {
                query = query.Where(x => x.StartDate <= dto.StartDateTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(dto.City))
            {
                query = query.Where(x =>
                    x.Days.Any(d =>
                        d.RouteDayBlocks.Any(rdb =>
                            rdb.Block != null && rdb.Block.City == dto.City)));
            }

            if (!string.IsNullOrWhiteSpace(dto.Search))
            {
                var search = dto.Search.Trim().ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.Title)
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
                    Budget = route.Budget,
                    FirstCity = route.Days
                        .SelectMany(d => d.RouteDayBlocks)
                        .Where(rdb => rdb.Block != null)
                        .Select(rdb => rdb.Block!.City)
                        .FirstOrDefault(),
                    IsPublic = route.IsPublic,
                    OwnerUsername = route.User != null ? route.User.UserName : null,
                    LikesCount = route.LikesCount,
                    IsLikedByCurrentUser = route.Likes.Any(l => l.UserId == userId)
                })
                .ToListAsync();

            return new PagedRoutesResponseDTO
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<RouteResponseDTO?> GetMyRouteAsync(string userId, int routeId)
        {
            var route = await GetUserRouteWithDetailsAsync(userId, routeId);
            return route == null ? null : MapRouteResponse(route, userId);
        }

        public async Task<RouteResponseDTO?> UpdateRouteMetaAsync(string userId, int routeId, UpdateRouteMetaDTO dto)
        {
            var route = await _dbContext.Routes
                .Include(x => x.Days)
                .FirstOrDefaultAsync(x => x.Id == routeId && x.UserId == userId);

            if (route == null)
            {
                return null;
            }

            if (dto.Title != null)
                route.Title = dto.Title;

            if (dto.Description != null)
                route.Description = dto.Description;

            if (dto.CoverEmoji != null)
                route.CoverEmoji = NormalizeCoverEmoji(dto.CoverEmoji);

            if (dto.Budget.HasValue)
            {
                ValidateBudget(dto.Budget);
                route.Budget = dto.Budget;
            }

            if (dto.StartDate.HasValue)
                route.StartDate = dto.StartDate.Value;

            if (dto.EndDate.HasValue)
                route.EndDate = dto.EndDate.Value;

            var startDate = route.StartDate;
            var endDate = route.EndDate;

            ValidateRouteDates(startDate, endDate);
            ValidateDayNumbers(route.Days.Select(x => x.DayNumber).ToList(), startDate, endDate);

            if (dto.IsPublic.HasValue)
            {
                route.IsPublic = dto.IsPublic.Value;

                
            }

            await _dbContext.SaveChangesAsync();

            return await GetMyRouteAsync(userId, routeId);
        }

        public async Task<bool> DeleteRouteAsync(string userId, int routeId)
        {
            var route = await _dbContext.Routes
                .FirstOrDefaultAsync(x => x.Id == routeId && x.UserId == userId);

            if (route == null)
            {
                return false;
            }

            _dbContext.Routes.Remove(route);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<RouteResponseDTO?> AddDayAsync(string userId, int routeId, CreateRouteDayRequestDTO dto)
        {
            var route = await _dbContext.Routes
                .Include(x => x.Days)
                .FirstOrDefaultAsync(x => x.Id == routeId && x.UserId == userId);

            if (route == null)
            {
                return null;
            }

            ValidateSingleDayNumber(
                dto.DayNumber,
                route.StartDate,
                route.EndDate);

            if (route.Days.Any(x => x.DayNumber == dto.DayNumber))
            {
                throw new InvalidOperationException("День с таким номером уже существует.");
            }

            var day = new RouteDay
            {
                RouteId = route.Id,
                DayNumber = dto.DayNumber,
                Title = dto.Title,
                Notes = dto.Notes
            };

            _dbContext.RouteDays.Add(day);
            await _dbContext.SaveChangesAsync();

            return await GetMyRouteAsync(userId, routeId);
        }

        public async Task<RouteResponseDTO?> UpdateDayAsync(string userId, int routeId, int dayId, UpdateRouteDayRequestDTO dto)
        {
            var route = await _dbContext.Routes
                .Include(x => x.Days)
                .FirstOrDefaultAsync(x => x.Id == routeId && x.UserId == userId);

            if (route == null)
            {
                return null;
            }

            var day = route.Days.FirstOrDefault(x => x.Id == dayId);
            if (day == null)
            {
                return null;
            }

            if (dto.DayNumber.HasValue)
            {
                ValidateSingleDayNumber(
                    dto.DayNumber.Value,
                    route.StartDate,
                    route.EndDate);

                var duplicate = route.Days.Any(x => x.Id != dayId && x.DayNumber == dto.DayNumber.Value);
                if (duplicate)
                {
                    throw new InvalidOperationException("День с таким номером уже существует.");
                }

                day.DayNumber = dto.DayNumber.Value;
            }

            if (dto.Title != null)
                day.Title = dto.Title;

            if (dto.Notes != null)
                day.Notes = dto.Notes;

            await _dbContext.SaveChangesAsync();

            return await GetMyRouteAsync(userId, routeId);
        }

        public async Task<bool> DeleteDayAsync(string userId, int routeId, int dayId)
        {
            var route = await _dbContext.Routes
                .Include(x => x.Days)
                .FirstOrDefaultAsync(x => x.Id == routeId && x.UserId == userId);

            if (route == null)
            {
                return false;
            }

            var day = route.Days.FirstOrDefault(x => x.Id == dayId);
            if (day == null)
            {
                return false;
            }

            var deletedDayNumber = day.DayNumber;

            _dbContext.RouteDays.Remove(day);

            var daysToRenumber = route.Days
                .Where(x => x.Id != dayId && x.DayNumber > deletedDayNumber)
                .OrderBy(x => x.DayNumber)
                .ToList();

            foreach (var item in daysToRenumber)
            {
                item.DayNumber--;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<RouteResponseDTO?> AddBlockAsync(string userId, int routeId, int dayId, AddRouteDayBlockDTO dto)
        {
            var day = await _dbContext.RouteDays
                .Include(x => x.Route)
                .Include(x => x.RouteDayBlocks)
                .FirstOrDefaultAsync(x => x.Id == dayId && x.RouteId == routeId && x.Route != null && x.Route.UserId == userId);

            if (day == null)
            {
                return null;
            }

            await EnsureApprovedBlocksAsync(new List<int> { dto.BlockId });

            if (dto.OrderInDay < 1)
            {
                throw new InvalidOperationException("OrderInDay должен быть больше нуля.");
            }

            var maxAllowedOrder = day.RouteDayBlocks.Count + 1;
            if (dto.OrderInDay > maxAllowedOrder)
            {
                throw new InvalidOperationException("OrderInDay выходит за допустимый диапазон.");
            }

            foreach (var block in day.RouteDayBlocks.Where(x => x.OrderInDay >= dto.OrderInDay))
            {
                block.OrderInDay++;
            }

            day.RouteDayBlocks.Add(new RouteDayBlock
            {
                BlockId = dto.BlockId,
                OrderInDay = dto.OrderInDay,
                Notes = dto.Notes
            });

            await _dbContext.SaveChangesAsync();

            return await GetMyRouteAsync(userId, routeId);
        }

        public async Task<RouteResponseDTO?> UpdateBlockAsync(string userId, int routeId, int dayId, int routeDayBlockId, UpdateRouteDayBlockDTO dto)
        {
            var day = await _dbContext.RouteDays
                .Include(x => x.Route)
                .Include(x => x.RouteDayBlocks)
                .FirstOrDefaultAsync(x => x.Id == dayId && x.RouteId == routeId && x.Route != null && x.Route.UserId == userId);

            if (day == null)
            {
                return null;
            }

            var routeDayBlock = day.RouteDayBlocks.FirstOrDefault(x => x.Id == routeDayBlockId);
            if (routeDayBlock == null)
            {
                return null;
            }

            if (dto.BlockId.HasValue)
            {
                await EnsureApprovedBlocksAsync(new List<int> { dto.BlockId.Value });
                routeDayBlock.BlockId = dto.BlockId.Value;
            }

            if (dto.Notes != null)
            {
                routeDayBlock.Notes = dto.Notes;
            }

            if (dto.OrderInDay.HasValue && dto.OrderInDay.Value != routeDayBlock.OrderInDay)
            {
                var newOrder = dto.OrderInDay.Value;

                if (newOrder < 1 || newOrder > day.RouteDayBlocks.Count)
                {
                    throw new InvalidOperationException("OrderInDay выходит за допустимый диапазон.");
                }

                ReorderBlocks(day.RouteDayBlocks.ToList(), routeDayBlock, newOrder);
            }

            await _dbContext.SaveChangesAsync();

            return await GetMyRouteAsync(userId, routeId);
        }

        public async Task<bool> DeleteBlockAsync(string userId, int routeId, int dayId, int routeDayBlockId)
        {
            var day = await _dbContext.RouteDays
                .Include(x => x.Route)
                .Include(x => x.RouteDayBlocks)
                .FirstOrDefaultAsync(x => x.Id == dayId && x.RouteId == routeId && x.Route != null && x.Route.UserId == userId);

            if (day == null)
            {
                return false;
            }

            var routeDayBlock = day.RouteDayBlocks.FirstOrDefault(x => x.Id == routeDayBlockId);
            if (routeDayBlock == null)
            {
                return false;
            }

            var deletedOrder = routeDayBlock.OrderInDay;

            _dbContext.RouteDayBlocks.Remove(routeDayBlock);

            foreach (var block in day.RouteDayBlocks.Where(x => x.Id != routeDayBlockId && x.OrderInDay > deletedOrder))
            {
                block.OrderInDay--;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task<Route?> GetUserRouteWithDetailsAsync(string userId, int routeId)
        {
            return await _dbContext.Routes
                .Include(x => x.User)
                .Include(x => x.Likes)
                .AsNoTracking()
                .Where(x => x.Id == routeId && x.UserId == userId)
                .Include(x => x.Days.OrderBy(d => d.DayNumber))
                    .ThenInclude(x => x.RouteDayBlocks.OrderBy(rdb => rdb.OrderInDay))
                        .ThenInclude(x => x.Block)
                .FirstOrDefaultAsync();
        }

        private static RouteResponseDTO MapRouteResponse(Route route, string userId)
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
                Budget = route.Budget,
                
                OwnerUsername = route.User?.UserName,
                LikesCount = route.LikesCount,
                IsLikedByCurrentUser = route.Likes.Any(like => like.UserId == userId),
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
                    .ToList(),
                
            };
        }

        private static void ValidateRouteDates(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            { 
                throw new InvalidOperationException("Дата начала маршрута не может быть позже даты окончания.");
            }
        }

        private static string? NormalizeCoverEmoji(string? coverEmoji)
        {
            return string.IsNullOrWhiteSpace(coverEmoji) ? null : coverEmoji.Trim();
        }

        private static void ValidateBudget(decimal? budget)
        {
            if (budget.HasValue && budget.Value < 0)
            {
                throw new InvalidOperationException("Бюджет не может быть отрицательным.");
            }
        }

        private static void ValidateCreateDays(List<CreateRouteDayDTO> days, DateOnly startDate, DateOnly endDate)
        {
            var dayNumbers = days.Select(x => x.DayNumber).ToList();
            ValidateDayNumbers(dayNumbers, startDate, endDate);

            var duplicateDays = dayNumbers
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateDays.Count > 0)
            {
                throw new InvalidOperationException($"Найдены дублирующиеся DayNumber: {string.Join(", ", duplicateDays)}");
            }

            foreach (var day in days)
            {
                var duplicateOrders = day.Blocks
                    .GroupBy(x => x.OrderInDay)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateOrders.Count > 0)
                {
                    throw new InvalidOperationException($"В дне {day.DayNumber} найдены дублирующиеся OrderInDay: {string.Join(", ", duplicateOrders)}");
                }
            }
        }

        private static void ValidateDayNumbers(List<int> dayNumbers, DateOnly startDate, DateOnly endDate)
        {
            var routeLength = endDate.DayNumber - startDate.DayNumber + 1;



            if (dayNumbers.Any(x => x < 1 || x > routeLength))
            {
                throw new InvalidOperationException("Номер дня выходит за диапазон маршрута.");
            }
        }

        private static void ValidateSingleDayNumber(int dayNumber, DateOnly startDate, DateOnly endDate)
        {
            ValidateDayNumbers(new List<int> { dayNumber }, startDate, endDate);
        }

        private async Task EnsureApprovedBlocksAsync(List<int> blockIds)
        {
            if (blockIds.Count == 0)
            {
                return;
            }

            var approvedIds = await _dbContext.Blocks
                .Where(x => blockIds.Contains(x.Id) && x.Status == BlockStatus.Approved)
                .Select(x => x.Id)
                .ToListAsync();

            var approvedSet = approvedIds.ToHashSet();
            var invalidIds = blockIds.Where(x => !approvedSet.Contains(x)).Distinct().ToList();

            if (invalidIds.Count > 0)
            {
                throw new InvalidOperationException($"Некоторые точки не найдены или не подтверждены: {string.Join(", ", invalidIds)}");
            }
        }

        private static void ReorderBlocks(List<RouteDayBlock> blocks, RouteDayBlock targetBlock, int newOrder)
        {
            var oldOrder = targetBlock.OrderInDay;

            if (newOrder > oldOrder)
            {
                foreach (var block in blocks.Where(x => x.Id != targetBlock.Id && x.OrderInDay > oldOrder && x.OrderInDay <= newOrder))
                {
                    block.OrderInDay--;
                }
            }
            else
            {
                foreach (var block in blocks.Where(x => x.Id != targetBlock.Id && x.OrderInDay >= newOrder && x.OrderInDay < oldOrder))
                {
                    block.OrderInDay++;
                }
            }

            targetBlock.OrderInDay = newOrder;
        }

        public async Task<bool> LikeRouteAsync(string userId, int routeId)
        {
            var route = await _dbContext.Routes
                .FirstOrDefaultAsync(x => x.Id == routeId);

            if (route == null)
            {
                return false;
            }

            var exists = await _dbContext.RouteLikes
                .AnyAsync(x => x.RouteId == routeId && x.UserId == userId);

            if (exists)
            {
                return true;
            }

            _dbContext.RouteLikes.Add(new RouteLike
            {
                RouteId = routeId,
                UserId = userId
            });
            route.LikesCount++;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikeRouteAsync(string userId, int routeId)
        {
            var like = await _dbContext.RouteLikes
                .FirstOrDefaultAsync(x => x.RouteId == routeId && x.UserId == userId);

            if (like == null)
            {
                return false;
            }

            var route = await _dbContext.Routes.FirstOrDefaultAsync(x => x.Id == routeId);
            if (route == null)
            {
                return false;
            }

            _dbContext.RouteLikes.Remove(like);

            if (route.LikesCount > 0)
            {
                route.LikesCount--;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }


        public async Task<PagedRoutesResponseDTO> GetLikedRoutesAsync(string userId, GetRoutesQueryDTO dto)
        {
            var page = dto.Page < 1 ? 1 : dto.Page;
            var pageSize = dto.PageSize < 1 ? 10 : dto.PageSize;
            if (pageSize > 50) pageSize = 50;

            var query = _dbContext.Routes
                .AsNoTracking()
                .Where(r => r.Likes.Any(l => l.UserId == userId));

            if (!string.IsNullOrWhiteSpace(dto.City))
            {
                query = query.Where(r => r.Days.Any(d =>
                    d.RouteDayBlocks.Any(rdb => rdb.Block != null && rdb.Block.City == dto.City)));
            }

            if (!string.IsNullOrWhiteSpace(dto.Search))
            {
                var search = dto.Search.Trim().ToLower();
                query = query.Where(r => r.Title.ToLower().Contains(search));
            }

            if (dto.StartDateFrom.HasValue)
            {
                query = query.Where(r => r.StartDate >= dto.StartDateFrom.Value);
            }

            if (dto.StartDateTo.HasValue)
            {
                query = query.Where(r => r.StartDate <= dto.StartDateTo.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(r => r.StartDate)
                .ThenBy(r => r.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RoutePreviewDTO
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    CoverEmoji = r.CoverEmoji,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Budget = r.Budget,
                    DaysCount = r.Days.Count,
                    PointsCount = r.Days.SelectMany(d => d.RouteDayBlocks).Count(),
                    FirstCity = r.Days
                        .SelectMany(d => d.RouteDayBlocks)
                        .Where(rdb => rdb.Block != null)
                        .Select(rdb => rdb.Block!.City)
                        .FirstOrDefault(),
                    IsPublic = r.IsPublic,
                    OwnerUsername = r.User != null ? r.User.UserName : null,
                    LikesCount = r.LikesCount,
                    IsLikedByCurrentUser = true
                })
                .ToListAsync();


            return new PagedRoutesResponseDTO
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        private async Task DeactivateAllShareLinksAsync(int routeId, DateTime now)
        {
            var links = await _dbContext.RouteShareLinks
                .Where(x => x.RouteId == routeId && x.IsActive)
                .ToListAsync();

            foreach (var link in links)
            {
                link.IsActive = false;
                link.RevokedAt ??= now;
            }

            if (links.Count > 0)
            {
                await _dbContext.SaveChangesAsync();
            }
        }

        private RouteShareLinkResponseDTO MapShareLink(RouteShareLink link)
        {
            return new RouteShareLinkResponseDTO
            {
                Token = link.Token,
                Url = BuildShareUrl(link.Token),
                ExpiresAt = link.ExpiresAt,
                IsActive = link.IsActive && link.RevokedAt == null && link.ExpiresAt > DateTime.UtcNow
            };
        }

        private string BuildShareUrl(string token)
        {
            var baseUrl = _configuration["ShareLinks:PublicBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("ShareLinks:PublicBaseUrl is not configured.");
            }

            return $"{baseUrl}/api/UnauthorizedRoute/by-token/{token}";
        }

        private int GetShareLifetimeDays()
        {
            var raw = _configuration["ShareLinks:LifetimeDays"];
            return int.TryParse(raw, out var days) && days > 0 ? days : 30;
        }

        private static string GenerateShareToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        

    }
}
