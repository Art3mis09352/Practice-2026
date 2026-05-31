using Application.DTO.Block;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services.MinIO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Swashbuckle.AspNetCore.Annotations;

namespace Practice.Controllers.UnauthorizedControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnauthorizedPointController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IObjectStorageService _objectStorageService;

        public UnauthorizedPointController(AppDbContext appDbContext, IObjectStorageService objectStorageService)
        {
            _appDbContext = appDbContext;
            _objectStorageService = objectStorageService;
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Получение информации о точке",
            Description = "Возвращает информацию о подтвержденной точке на основе ее идентификатора."
        )]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlockResponseDTO>> GetPointInfo(int id)
        {
            var point = await _appDbContext.Blocks
                .AsNoTracking()
                .Include(b => b.Photos)
                .FirstOrDefaultAsync(x => x.Id == id && x.Status == BlockStatus.Approved);

            if (point == null)
            {
                return NotFound();
            }
            var orderedPhotos = point.Photos.OrderBy(p => p.Id).ToList();
            var previewPhoto = orderedPhotos.FirstOrDefault(p => p.Id == point.PreviewPhotoId)
                ?? orderedPhotos.FirstOrDefault();

            var result = new BlockResponseDTO
            {
                Id = point.Id,
                OwnerId = point.OwnerId,
                Title = point.Title,
                Description = point.Description,
                Category = point.Category,
                City = point.City,
                Address = point.Address,
                Latitude = point.Latitude,
                Longitude = point.Longitude,
                AvgPrice = point.AvgPrice,
                Status = point.Status,
                PreviewPhotoId = previewPhoto?.Id,
                PreviewPhotoUrl = previewPhoto == null ? null : _objectStorageService.GetBlockPhotoPublicUrl(previewPhoto.ObjectName),
                Photos = orderedPhotos.Select(p => new BlockPhotoDTO
                {
                    Id = p.Id,
                    Url = _objectStorageService.GetBlockPhotoPublicUrl(p.ObjectName)
                }).ToList()
            };

            return Ok(result);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Получение информации о точках",
            Description = "Возвращает информацию о точках для всех пользователей."
        )]
        public async Task<ActionResult<PagedBlocksResponseDTO>> GetBlocks([FromQuery] GetBlocksQueryDTO queryDto)
        {
            var page = queryDto.Page < 1 ? 1 : queryDto.Page;
            var pageSize = queryDto.PageSize < 1 ? 10 : queryDto.PageSize;
            if (pageSize > 50) pageSize = 50;

            var query = _appDbContext.Blocks
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
