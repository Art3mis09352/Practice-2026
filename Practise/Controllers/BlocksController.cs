using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Data;
using Practice.Data.DTO.Block;

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
        public async Task<ActionResult<IEnumerable<BlockPreviewDTO>>> GetBlocks(
            [FromQuery] string? city,
            [FromQuery] string? category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var query = _dbContext.Blocks
                .Where(b => b.IsApproved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(b => b.City == city);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(b => b.Category == category);
            }

            var blocks = await query
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

            return Ok(blocks);
        }
    }
}
