using Application.DTO.Admin;
using Application.DTO.Block;
using Application.DTO.Common;
using Application.DTO.User;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services.MinIO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly IObjectStorageService _objectStorageService;

        public AdminController(UserManager<User> userManager, AppDbContext dbContext, IObjectStorageService objectStorageService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _objectStorageService = objectStorageService;
        }

        [HttpGet("blocks")]
        [SwaggerOperation(
            Summary = "Получить все точки",
            Description = "Возвращает список всех точек для администратора с пагинацией и поиском по названию или адресу."
        )]
        [ProducesResponseType(typeof(PagedBlocksResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedBlocksResponseDTO>> GetBlocks([FromQuery] GetAdminBlocksQueryDTO dto)
        {
            return Ok(await GetBlocksResponseAsync(dto, pendingOnly: false));
        }

        [HttpGet("blocks/pending")]
        [SwaggerOperation(
            Summary = "Получить точки на модерации",
            Description = "Возвращает список неподтвержденных точек с пагинацией и поиском по названию или адресу."
        )]
        [ProducesResponseType(typeof(PagedBlocksResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedBlocksResponseDTO>> GetPendingBlocks([FromQuery] GetAdminBlocksQueryDTO dto)
        {
            return Ok(await GetBlocksResponseAsync(dto, pendingOnly: true));
        }

        [HttpGet("users")]
        [SwaggerOperation(
            Summary = "Получить пользователей",
            Description = "Возвращает список пользователей для администратора с пагинацией и поиском по username или email."
        )]
        [ProducesResponseType(typeof(PagedAdminUsersResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedAdminUsersResponseDTO>> GetUsers([FromQuery] GetAdminUsersQueryDTO dto)
        {
            var page = dto.Page < 1 ? 1 : dto.Page;
            var pageSize = dto.PageSize < 1 ? 10 : dto.PageSize;
            if (pageSize > 50) pageSize = 50;

            var query = _userManager.Users
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Search))
            {
                var search = dto.Search.Trim().ToLower();
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                    (u.Email != null && u.Email.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.UserName)
                .ThenBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = new List<UserInfoResponseDTO>(users.Count);
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                items.Add(new UserInfoResponseDTO
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    Username = user.UserName ?? string.Empty,
                    Phone = user.PhoneNumber,
                    Roles = roles
                });
            }

            return Ok(new PagedAdminUsersResponseDTO
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }

        [HttpDelete("users/{id}")]
        [SwaggerOperation(
            Summary = "Удалить пользователя",
            Description = "Админ удаляет пользователя по идентификатору."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeleteUserByRoute(string id)
        {
            return await DeleteUserInternalAsync(id);
        }

        [HttpDelete("DeleteUser")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> DeleteUser(string id)
        {
            return await DeleteUserInternalAsync(id);
        }

        [HttpDelete("blocks/{id:int}")]
        [SwaggerOperation(
            Summary = "Удалить точку",
            Description = "Админ удаляет точку по идентификатору."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteBlock(int id, CancellationToken cancellationToken = default)
        {
            var block = await _dbContext.Blocks
                .Include(x => x.Photos)
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (block == null)
            {
                return NotFound("Точка не найдена.");
            }

            foreach (var photo in block.Photos)
            {
                await _objectStorageService.DeleteBlockPhotoAsync(photo.ObjectName);
            }
            foreach (var document in block.Documents)
            {
                await _objectStorageService.DeleteBlockDocumentAsync(document.ObjectName, cancellationToken);
            }
            _dbContext.Blocks.Remove(block);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("blocks/{id:int}/approve")]
        [SwaggerOperation(
            Summary = "Подтвердить точку",
            Description = "Админ подтверждает точку, снимая ее с модерации."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ApproveBlock(int id)
        {
            var block = await _dbContext.Blocks.FindAsync(id);
            if (block == null)
            {
                return NotFound("Точка не найдена.");
            }

            if (!block.IsApproved)
            {
                block.IsApproved = true;
                await _dbContext.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpGet("blocks/{id:int}")]
        [SwaggerOperation(
            Summary = "Получить точку",
            Description = "Возвращает полную информацию о точке для администратора."
        )]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlockResponseDTO>> GetBlock(int id)
        {
            var block = await _dbContext.Blocks
                .AsNoTracking()
                .Include(x => x.Photos)
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (block == null)
            {
                return NotFound("Точка не найдена.");
            }

            var orderedPhotos = block.Photos.OrderBy(p => p.Id).ToList();
            var previewPhoto = orderedPhotos.FirstOrDefault(p => p.Id == block.PreviewPhotoId)
                ?? orderedPhotos.FirstOrDefault();

            return Ok(new BlockResponseDTO
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
                PreviewPhotoId = previewPhoto?.Id,
                PreviewPhotoUrl = previewPhoto == null
                    ? null
                    : _objectStorageService.GetBlockPhotoPublicUrl(previewPhoto.ObjectName),
                            Photos = orderedPhotos
                    .Select(p => new BlockPhotoDTO
                    {
                        Id = p.Id,
                        Url = _objectStorageService.GetBlockPhotoPublicUrl(p.ObjectName)
                    })
                    .ToList(),
                Documents = block.Documents
                    .OrderBy(x => x.Id)
                    .Select(x => new BlockDocumentDTO
                    {
                        Id = x.Id,
                        OriginalFileName = x.OriginalFileName,
                        ContentType = x.ContentType,
                        Size = x.Size,
                        CreatedAt = x.CreatedAt
                    })
                    .ToList()
            });
        }

        [HttpGet("blocks/{blockId:int}/documents/{documentId:int}/download-url")]
        [SwaggerOperation(
            Summary = "Временная ссылка на документ точки",
            Description = "Админ получает временную ссылку на документ точки."
        )]
        [ProducesResponseType(typeof(DownloadUrlResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DownloadUrlResponseDTO>> GetBlockDocumentDownloadUrl(
            int blockId,
            int documentId,
            CancellationToken cancellationToken)
        {
            var document = await _dbContext.BlockDocuments
                .FirstOrDefaultAsync(x => x.Id == documentId && x.BlockId == blockId, cancellationToken);

            if (document == null)
            {
                return NotFound("Документ не найден.");
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



        private async Task<ActionResult> DeleteUserInternalAsync(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(currentUserId) && currentUserId == id)
            {
                return BadRequest("Администратор не может удалить сам себя.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            await DeleteUserDependenciesAsync(user.Id);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(x => x.Description));
            }

            return NoContent();
        }

        private async Task<PagedBlocksResponseDTO> GetBlocksResponseAsync(GetAdminBlocksQueryDTO dto, bool pendingOnly)
        {
            var page = dto.Page < 1 ? 1 : dto.Page;
            var pageSize = dto.PageSize < 1 ? 10 : dto.PageSize;
            if (pageSize > 50) pageSize = 50;

            var query = _dbContext.Blocks
                .AsNoTracking()
                .AsQueryable();

            if (pendingOnly)
            {
                query = query.Where(b => !b.IsApproved);
            }

            if (!string.IsNullOrWhiteSpace(dto.Search))
            {
                var search = dto.Search.Trim().ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(search) ||
                    (b.Address != null && b.Address.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.IsApproved)
                .ThenBy(b => b.Title)
                .ThenBy(b => b.Id)
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
                    IsApproved = b.IsApproved,
                    PreviewPhotoUrl = b.Photos
                        .Where(p => b.PreviewPhotoId.HasValue ? p.Id == b.PreviewPhotoId.Value : true)
                        .OrderBy(p => p.Id)
                        .Select(p => _objectStorageService.GetBlockPhotoPublicUrl(p.ObjectName))
                        .FirstOrDefault(),
                    PhotosCount = b.Photos.Count()
                })
                .ToListAsync();

            return new PagedBlocksResponseDTO
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        private async Task DeleteUserDependenciesAsync(string userId)
        {
            var routeLikes = await _dbContext.RouteLikes
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var routes = await _dbContext.Routes
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var blocks = await _dbContext.Blocks
                .Include(x => x.Photos)
                .Include(x => x.Documents)
                .Where(x => x.OwnerId == userId)
                .ToListAsync();

            if (routeLikes.Count > 0)
            {
                _dbContext.RouteLikes.RemoveRange(routeLikes);
            }

            if (routes.Count > 0)
            {
                _dbContext.Routes.RemoveRange(routes);
            }

            if (blocks.Count > 0)
            {
                foreach (var block in blocks)
                {
                    foreach (var photo in block.Photos)
                    {
                        await _objectStorageService.DeleteBlockPhotoAsync(photo.ObjectName);
                    }
                    foreach (var document in block.Documents)
                    {
                        await _objectStorageService.DeleteBlockDocumentAsync(document.ObjectName);
                    }
                }

                _dbContext.Blocks.RemoveRange(blocks);
            }

            if (routeLikes.Count > 0 || routes.Count > 0 || blocks.Count > 0)
            {
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
