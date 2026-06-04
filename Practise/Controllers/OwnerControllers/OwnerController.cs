using Application.DTO.Block;
using Application.DTO.Common;
using Domain.Entities;
using Domain.Enums;
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

        private static readonly HashSet<string> AllowedDocumentContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf"
        };

        private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf"
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
                .Include(b => b.Documents)
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
                Status = BlockStatus.Pending
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

            block.Status = BlockStatus.Pending;
            block.ModerationComment = null;
            block.ModeratedAt = null;
            block.ModeratedByUserId = null;

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

            block.Status = BlockStatus.Pending;
            block.ModerationComment = null;
            block.ModeratedAt = null;
            block.ModeratedByUserId = null;
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (block.PreviewPhotoId == null)
            {
                var firstPhotoId = block.Photos.OrderBy(x => x.Id).Select(x => x.Id).FirstOrDefault();
                if (firstPhotoId != 0)
                {
                    block.PreviewPhotoId = firstPhotoId;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }

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
            var wasPreview = photo.Block.PreviewPhotoId == photo.Id;
            await _objectStorageService.DeleteBlockPhotoAsync(photo.ObjectName, cancellationToken);

            photo.Block.Status = BlockStatus.Pending;
            photo.Block.ModerationComment = null;
            photo.Block.ModeratedAt = null;
            photo.Block.ModeratedByUserId = null;
            _dbContext.BlockPhotos.Remove(photo);
            if (wasPreview)
            {
                var nextPreviewId = await _dbContext.BlockPhotos
                    .Where(x => x.BlockId == blockId && x.Id != photoId)
                    .OrderBy(x => x.Id)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                photo.Block.PreviewPhotoId = nextPreviewId;
            }
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
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId, cancellationToken);

            if (block == null)
            {
                return NotFound();
            }

            foreach (var photo in block.Photos)
            {
                await _objectStorageService.DeleteBlockPhotoAsync(photo.ObjectName, cancellationToken);
            }
            foreach (var document in block.Documents)
            {
                await _objectStorageService.DeleteBlockDocumentAsync(document.ObjectName, cancellationToken);
            }

            if (block.PreviewPhotoId != null)
            {
                block.PreviewPhotoId = null;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            _dbContext.Blocks.Remove(block);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        [HttpPatch("blocks/{blockId:int}/photos/{photoId:int}/preview")]
        [SwaggerOperation(
            Summary = "Установить превью фото",
            Description = "Назначает выбранное фото превью-изображением точки владельца."
        )]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlockResponseDTO>> SetPreviewPhoto(
            int blockId,
            int photoId,
            CancellationToken cancellationToken)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            var block = await _dbContext.Blocks
                .Include(x => x.Photos)
                .FirstOrDefaultAsync(x => x.Id == blockId && x.OwnerId == ownerId, cancellationToken);

            if (block == null)
            {
                return NotFound();
            }

            var photoExists = block.Photos.Any(x => x.Id == photoId);
            if (!photoExists)
            {
                return NotFound("Фото не найдено в этой точке.");
            }

            block.PreviewPhotoId = photoId;
            block.Status = BlockStatus.Pending;
            block.ModerationComment = null;
            block.ModeratedAt = null;
            block.ModeratedByUserId = null;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(MapBlockResponse(block));
        }

        [HttpPost("blocks/{blockId:int}/documents")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Загрузка документа точки",
            Description = "Owner загружает PDF-документ к своей точке."
        )]
        [ProducesResponseType(typeof(IReadOnlyCollection<BlockDocumentDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyCollection<BlockDocumentDTO>>> UploadDocuments(
            int blockId,
            [FromForm] List<IFormFile> files,
            CancellationToken cancellationToken)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            var block = await _dbContext.Blocks
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.Id == blockId && x.OwnerId == ownerId, cancellationToken);

            if (block == null)
            {
                return NotFound();
            }

            if (files == null || files.Count == 0)
            {
                return BadRequest("Нужно передать хотя бы один PDF-файл.");
            }

            foreach (var file in files)
            {
                if (file.Length <= 0)
                {
                    return BadRequest("Один из файлов пустой.");
                }

                if (!AllowedDocumentContentTypes.Contains(file.ContentType) ||
                    !AllowedDocumentExtensions.Contains(Path.GetExtension(file.FileName)))
                {
                    return BadRequest("Разрешены только PDF-документы.");
                }
            }

            foreach (var file in files)
            {
                await using var stream = file.OpenReadStream();

                var objectName = await _objectStorageService.UploadBlockDocumentAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    cancellationToken);

                block.Documents.Add(new BlockDocument
                {
                    ObjectName = objectName,
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    Size = file.Length
                });
            }

            block.Status = BlockStatus.Pending;
            block.ModerationComment = null;
            block.ModeratedAt = null;
            block.ModeratedByUserId = null;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(block.Documents
                .OrderBy(x => x.Id)
                .Select(MapDocument)
                .ToList());
        }

        [HttpDelete("blocks/{blockId:int}/documents/{documentId:int}")]
        [SwaggerOperation(
            Summary = "Удаление документа точки",
            Description = "Owner удаляет документ своей точки."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDocument(
            int blockId,
            int documentId,
            CancellationToken cancellationToken)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            var document = await _dbContext.BlockDocuments
                .Include(x => x.Block)
                .FirstOrDefaultAsync(x => x.Id == documentId && x.BlockId == blockId, cancellationToken);

            if (document == null || document.Block == null || document.Block.OwnerId != ownerId)
            {
                return NotFound();
            }

            await _objectStorageService.DeleteBlockDocumentAsync(document.ObjectName, cancellationToken);

            document.Block.Status = BlockStatus.Pending;
            document.Block.ModerationComment = null;
            document.Block.ModeratedAt = null;
            document.Block.ModeratedByUserId = null;

            _dbContext.BlockDocuments.Remove(document);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }


        private BlockResponseDTO MapBlockResponse(Block block)
        {
            var orderedPhotos = block.Photos
                .OrderBy(x => x.Id)
                .ToList();

            var previewPhoto = orderedPhotos.FirstOrDefault(x => x.Id == block.PreviewPhotoId)
                ?? orderedPhotos.FirstOrDefault();

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
                Status = block.Status,
                ModerationComment = block.ModerationComment,
                ModeratedAt = block.ModeratedAt,
                ModeratedByUserId = block.ModeratedByUserId,
                PreviewPhotoId = previewPhoto?.Id,
                PreviewPhotoUrl = previewPhoto == null
                    ? null
                    : _objectStorageService.GetBlockPhotoPublicUrl(previewPhoto.ObjectName),
                Photos = orderedPhotos
                    .Select(MapPhoto)
                    .ToList(),
                Documents = block.Documents
                    .OrderBy(x => x.Id)
                    .Select(MapDocument)
                    .ToList()
            };
        }

        [HttpGet("blocks/{blockId:int}/documents/{documentId:int}/download-url")]
        [SwaggerOperation(
            Summary = "Временная ссылка на документ",
            Description = "Owner получает временную ссылку на свой документ."
        )]
        [ProducesResponseType(typeof(DownloadUrlResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DownloadUrlResponseDTO>> GetDocumentDownloadUrl(
            int blockId,
            int documentId,
            CancellationToken cancellationToken)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            var document = await _dbContext.BlockDocuments
                .Include(x => x.Block)
                .FirstOrDefaultAsync(x => x.Id == documentId && x.BlockId == blockId, cancellationToken);

            if (document == null || document.Block == null || document.Block.OwnerId != ownerId)
            {
                return NotFound();
            }

            var (url, expiresAtUtc) = await _objectStorageService.GetBlockDocumentDownloadUrlAsync(
                document.ObjectName,
                TimeSpan.FromMinutes(10),
                cancellationToken);

            return Ok(new DownloadUrlResponseDTO
            {
                Url = url,
                ExpiresAtUtc = expiresAtUtc
            });
        }
        private BlockPhotoDTO MapPhoto(BlockPhoto photo)
        {
            return new BlockPhotoDTO
            {
                Id = photo.Id,
                Url = _objectStorageService.GetBlockPhotoPublicUrl(photo.ObjectName)
            };
        }
        private static BlockDocumentDTO MapDocument(BlockDocument document)
        {
            return new BlockDocumentDTO
            {
                Id = document.Id,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                Size = document.Size,
                CreatedAt = document.CreatedAt
            };
        }
    }
}
