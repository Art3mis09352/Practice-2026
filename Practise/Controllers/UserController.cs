using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Practice.Data;
using Practice.Data.DTO.Auth;
using Practice.Data.DTO.User;
using Practice.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public UserController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }


        [HttpGet("GetUserInfo")]
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

            var response = new UserInfoResponseDTO
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                Phone = user.PhoneNumber
            };
            return Ok(response);
        }




        [HttpPut("PutUserInfo")]
        [SwaggerOperation(
            Summary = "меняем номер телефона",
            Description = "меняем номер телефона."
        )]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateUserInfo([FromBody] UserDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userId)) {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            user.UserName = string.IsNullOrWhiteSpace(dto.Username) ? user.UserName : dto.Username;
            user.PhoneNumber = string.IsNullOrWhiteSpace(dto.Phone) ? user.PhoneNumber : dto.Phone;

            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {
                return BadRequest("Не удалось обновить информацию о пользователе.");
            }
            return NoContent();
        }
    }
}
