using Application.DTO.Block;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers.OwnerControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class GetMyPointsController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public GetMyPointsController(AppDbContext dbContext)
        {
            _appDbContext = dbContext;
        }

        [HttpGet("mypoints")]
        [SwaggerOperation(
            Summary = "Получение точек владельца",
            Description = "Возвращает список точек, принадлежащих владельцу."
        )]
        [ProducesResponseType(typeof(PagedBlocksResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedBlocksResponseDTO>> GetBlocks([FromQuery] GetBlocksQueryDTO queryDto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ownerId == null || !User.IsInRole("Owner"))
            {
                return Unauthorized();
            }
            var page = queryDto.Page < 1 ? 1 : queryDto.Page;
            var pageSize = queryDto.PageSize < 1 ? 10 : queryDto.PageSize;
            if (pageSize > 50) pageSize = 50;

            var query = _appDbContext.Blocks
                .AsNoTracking()
                .Where(b => b.OwnerId == ownerId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryDto.City))
            {
                query = query.Where(b => b.City == queryDto.City);
            }

            if (!string.IsNullOrWhiteSpace(queryDto.Category))
            {
                query = query.Where(b => b.Category == queryDto.Category);
            }

            var totalCount = await query.CountAsync();

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
                .ToListAsync();

            var response = new PagedBlocksResponseDTO
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(response);
        }
    }
}