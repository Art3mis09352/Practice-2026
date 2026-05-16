using Application.DTO.User;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services.Auth;
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

        public UserController(
            UserManager<User> userManager,
            AppDbContext dbContext,
            AuthCookieService authCookieService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _authCookieService = authCookieService;
        }

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
