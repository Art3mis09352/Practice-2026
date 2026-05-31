using Application.DTO.Block;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services.MinIO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlocksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IObjectStorageService _objectStorageService;

        public BlocksController(AppDbContext dbContext, IObjectStorageService objectStorageService)
        {
            _dbContext = dbContext;
            _objectStorageService = objectStorageService;
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
                .Where(b => b.Status == BlockStatus.Approved)
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

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BlockPreviewDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    Category = b.Category,
                    City = b.City,
                    Address = b.Address,
                    Latitude = b.Latitude,
                    Longitude = b.Longitude,
                    Status = b.Status,
                    PreviewPhotoUrl = b.Photos
                        .Where(p => b.PreviewPhotoId.HasValue ? p.Id == b.PreviewPhotoId.Value : true)
                        .OrderBy(p => p.Id)
                        .Select(p => _objectStorageService.GetBlockPhotoPublicUrl(p.ObjectName))
                        .FirstOrDefault(),
                    PhotosCount = b.Photos.Count()
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
