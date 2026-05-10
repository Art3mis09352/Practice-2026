using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Data;
using Practice.Data.DTO.Block;
using Swashbuckle.AspNetCore.Annotations;

namespace Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlocksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public BlocksController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Получение списка блоков", Description = "Возвращает список блоков с возможностью фильтрации по городу и категории.")]
        public async Task<ActionResult<PagedBlocksResponseDTO>> GetBlocks([FromQuery] GetBlocksQueryDTO queryDto)
        {
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

            return Ok(new PagedBlocksResponseDTO
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }
    }
}
