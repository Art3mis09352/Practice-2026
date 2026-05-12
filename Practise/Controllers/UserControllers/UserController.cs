using Application.Data.DTO.Route.Read;
using Application.DTO.Auth;
using Application.DTO.User;
using Domain.Entities;
using Infrastructure.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IUserRouteService _userRouteService;

        public UserController(UserManager<User> userManager, IUserRouteService userRouteService)
        {
            _userManager = userManager;
            _userRouteService = userRouteService;
        }


        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Получение информации о пользователе",
            Description = "Возвращает информацию о пользователе на основе его идентификатора."
        )]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
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
        

    }
}
