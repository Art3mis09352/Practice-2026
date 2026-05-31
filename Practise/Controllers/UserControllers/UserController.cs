using Application.DTO.Block;
using Application.DTO.User;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services.Auth;
using Infrastructure.Services.MinIO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers.UserControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly AuthCookieService _authCookieService;
        private readonly IObjectStorageService _objectStorageService;


        public UserController(
            UserManager<User> userManager,
            AppDbContext dbContext,
            AuthCookieService authCookieService, IObjectStorageService objectStorageService
        )
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _authCookieService = authCookieService;
            _objectStorageService = objectStorageService;
        }

        private static readonly HashSet<string> AllowedPhotoContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp"
        };

        private static readonly HashSet<string> AllowedPhotoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };




        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Получение информации о пользователе",
            Description = "Возвращает информацию о пользователе на основе его идентификатора."
        )]
        [ProducesResponseType(typeof(UserInfoResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserInfoResponseDTO>> GetUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var response = new UserInfoResponseDTO
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                Phone = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                IsConfirmed = user.EmailConfirmed,
                Roles = roles
            };

            return Ok(response);
        }

        [HttpPut("me")]
        [SwaggerOperation(
            Summary = "Обновление профиля пользователя",
            Description = "Обновляет username и телефон пользователя."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateUserInfo([FromBody] UpdateUserProfileDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            if (dto.Username != null)
            {
                if (string.IsNullOrWhiteSpace(dto.Username))
                {
                    return BadRequest("Username не может быть пустым.");
                }

                user.UserName = dto.Username.Trim();
            }

            if (dto.Phone != null)
            {
                user.PhoneNumber = string.IsNullOrWhiteSpace(dto.Phone)
                    ? null
                    : dto.Phone.Trim();
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(x => x.Description));
            }

            return NoContent();
        }

        [HttpPatch("me/password")]
        [SwaggerOperation(
            Summary = "Смена пароля",
            Description = "Позволяет текущему пользователю сменить пароль."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            if (dto.CurrentPassword == dto.NewPassword)
            {
                return BadRequest("Новый пароль должен отличаться от текущего.");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(x => x.Description));
            }

            _authCookieService.ClearAuthCookie(Response);
            return NoContent();
        }

        [HttpDelete("me")]
        [SwaggerOperation(
            Summary = "Удаление аккаунта",
            Description = "Удаляет аккаунт текущего пользователя вместе со связанными маршрутами, лайками и точками владельца."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
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

            _authCookieService.ClearAuthCookie(Response);
            return NoContent();
        }

        [Authorize]
        [HttpPost("blocks/suggest")]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status201Created)]
        public async Task<ActionResult<BlockResponseDTO>> SuggestBlock(
        [FromBody] SuggestBlockRequestDTO dto,
        CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var block = new Block
            {
                OwnerId = userId,
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                City = dto.City,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                AvgPrice = null,
                Status = BlockStatus.Pending,
                ModerationComment = null,
                ModeratedAt = null,
                ModeratedByUserId = null
            };

            _dbContext.Blocks.Add(block);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return StatusCode(StatusCodes.Status201Created, new BlockResponseDTO
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
                PreviewPhotoId = null,
                PreviewPhotoUrl = null,
                Photos = Array.Empty<BlockPhotoDTO>(),
                Documents = Array.Empty<BlockDocumentDTO>()
            });
        }



        [Authorize]
        [HttpPost("blocks/{id:int}/photos")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Загрузка фото для точки",
            Description = "Позволяет загрузить одно или несколько фото для точки."
        )]
        [ProducesResponseType(typeof(IReadOnlyCollection<BlockPhotoDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyCollection<BlockPhotoDTO>>> UploadSuggestedBlockPhotos(
            int id,
            [FromForm] List<IFormFile> files,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var block = await _dbContext.Blocks
                .Include(x => x.Photos)
                .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId, cancellationToken);

            if (block == null)
            {
                return NotFound("Точка не найдена.");
            }

            if (block.Status == BlockStatus.Approved)
            {
                return BadRequest("Нельзя добавлять фото к уже подтвержденной точке.");
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

                if (!AllowedPhotoContentTypes.Contains(file.ContentType) ||
                    !AllowedPhotoExtensions.Contains(Path.GetExtension(file.FileName)))
                {
                    return BadRequest("Разрешены только изображения JPG, PNG и WEBP.");
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

            return Ok(block.Photos
                .OrderBy(x => x.Id)
                .Select(x => new BlockPhotoDTO
                {
                    Id = x.Id,
                    Url = _objectStorageService.GetBlockPhotoPublicUrl(x.ObjectName)
                })
                .ToList());
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
                _dbContext.Blocks.RemoveRange(blocks);
            }

            if (routeLikes.Count > 0 || routes.Count > 0 || blocks.Count > 0)
            {
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
