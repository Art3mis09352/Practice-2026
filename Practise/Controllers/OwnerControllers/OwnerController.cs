using Application.DTO.Block;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services.MinIO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers.OwnerControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class OwnerController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IObjectStorageService _objectStorageService;

        public OwnerController(AppDbContext dbContext, IObjectStorageService objectStorageService)
        {
            _dbContext = dbContext;
            _objectStorageService = objectStorageService;
        }

        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpg",
            "image/jpeg",
            "image/png",
            "image/webp",
        };

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
        };

        [HttpGet("blocks/{id:int}")]
        [SwaggerOperation(
            Summary = "Получение точки владельца",
            Description = "Возвращает полную информацию о точке, принадлежащей текущему владельцу."
        )]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlockResponseDTO>> GetBlock(int id, CancellationToken cancellationToken)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            var block = await _dbContext.Blocks
                .AsNoTracking()
                .Include(b => b.Photos)
                .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId, cancellationToken);

            if (block == null)
            {
                return NotFound();
            }

            return Ok(MapBlockResponse(block));
        }

        [HttpPost("blocks")]
        [SwaggerOperation(
            Summary = "Точка",
            Description = "Создает новую точку/блок."
        )]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BlockResponseDTO>> CreateBlock([FromBody] CreateBlockRequestDTO dto, CancellationToken cancellationToken)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            var block = new Block
            {
                OwnerId = ownerId,
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                City = dto.City,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                AvgPrice = dto.AvgPrice,
                IsApproved = false
            };

            _dbContext.Blocks.Add(block);
            await _dbContext.SaveChangesAsync(cancellationToken);


            return CreatedAtAction(nameof(CreateBlock), new { id = block.Id }, MapBlockResponse(block));
        }

        [HttpPatch("blocks/{id:int}")]
        [SwaggerOperation(
            Summary = "Обновление точки",
            Description = "Обновляет существующую точку/блок."
        )]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status200OK)]
                [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BlockResponseDTO>> UpdateBlock(int id, [FromBody] UpdateBlockRequestDTO dto, CancellationToken cancellationToken)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }
            var block = await _dbContext.Blocks
                                .Include(b => b.Photos)
                                .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId, cancellationToken);
            if (block == null || block.OwnerId != ownerId)
            {
                return NotFound();
            }
            block.Title = dto.Title ?? block.Title;
            block.Description = dto.Description ?? block.Description;
            block.Category = dto.Category ?? block.Category;
            block.City = dto.City ?? block.City;
            block.Address = dto.Address ?? block.Address;
            block.Latitude = dto.Latitude ?? block.Latitude;
            block.Longitude = dto.Longitude ?? block.Longitude;
            block.AvgPrice = dto.AvgPrice ?? block.AvgPrice;

            block.IsApproved = false;
            
            await _dbContext.SaveChangesAsync();

            
            return Ok(MapBlockResponse(block));
        }
        [HttpPost("blocks/{id:int}/photos")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Загрузка фото точки",
            Description = "Добавляет одно или несколько изображений к точке владельца."
        )]
        [ProducesResponseType(typeof(IReadOnlyCollection<BlockPhotoDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyCollection<BlockPhotoDTO>>> UploadPhotos(
            int id,
            [FromForm] List<IFormFile> files,
            CancellationToken cancellationToken)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            var block = await _dbContext.Blocks
                .Include(x => x.Photos)
                .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId, cancellationToken);

            if (block == null)
            {
                return NotFound();
            }

            if (files == null || files.Count == 0)
            {
                return BadRequest("Нужно передать хотя бы один файл.");
            }

            foreach (var file in files)
            {
                if (file.Length <= 0)
                {
                    return BadRequest("Один из файлов пустой.");
                }

                if (!AllowedImageContentTypes.Contains(file.ContentType) || !AllowedImageExtensions.Contains(Path.GetExtension(file.FileName)))
                {
                    return BadRequest("Разрешены только изображения в форматах jpeg, png и webp.");
                }
            }

            foreach (var file in files)
            {
                await using var stream = file.OpenReadStream();

                var objectName = await _objectStorageService.UploadBlockPhotoAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    cancellationToken);

                block.Photos.Add(new BlockPhoto
                {
                    ObjectName = objectName,
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    Size = file.Length
                });
            }

            block.IsApproved = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = block.Photos
                .OrderBy(x => x.Id)
                .Select(MapPhoto)
                .ToList();

            return Ok(response);
        }

        [HttpDelete("blocks/{blockId:int}/photos/{photoId:int}")]
        [SwaggerOperation(
            Summary = "Удаление фото точки",
            Description = "Удаляет изображение из точки владельца."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePhoto(
            int blockId,
            int photoId,
            CancellationToken cancellationToken)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            var photo = await _dbContext.BlockPhotos
                .Include(x => x.Block)
                .FirstOrDefaultAsync(x => x.Id == photoId && x.BlockId == blockId, cancellationToken);

            if (photo == null || photo.Block == null || photo.Block.OwnerId != ownerId)
            {
                return NotFound();
            }

            await _objectStorageService.DeleteObjectAsync(photo.ObjectName, cancellationToken);

            photo.Block.IsApproved = false;
            _dbContext.BlockPhotos.Remove(photo);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        [HttpDelete("blocks/{id:int}")]
        [SwaggerOperation(
            Summary = "Удаление точки",
            Description = "Удаляет существующую точку/блок."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
                [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBlock(int id, CancellationToken cancellationToken)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            var block = await _dbContext.Blocks
                .Include(x => x.Photos)
                .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId, cancellationToken);

            if (block == null)
            {
                return NotFound();
            }

            foreach (var photo in block.Photos)
            {
                await _objectStorageService.DeleteObjectAsync(photo.ObjectName, cancellationToken);
            }

            _dbContext.Blocks.Remove(block);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        private BlockResponseDTO MapBlockResponse(Block block)
        {
            return new BlockResponseDTO
            {
                Id = block.Id,
                OwnerId = block.OwnerId,
                Title = block.Title,
                Description = block.Description,
                Category = block.Category,
                City = block.City,
                Address = block.Address,
                Latitude = block.Latitude,
                Longitude = block.Longitude,
                AvgPrice = block.AvgPrice,
                IsApproved = block.IsApproved,
                Photos = block.Photos
                    .OrderBy(x => x.Id)
                    .Select(MapPhoto)
                    .ToList()
            };
        }
        private BlockPhotoDTO MapPhoto(BlockPhoto photo)
        {
            return new BlockPhotoDTO
            {
                Id = photo.Id,
                Url = _objectStorageService.GetPublicUrl(photo.ObjectName)
            };
        }
    }
}
