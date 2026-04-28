using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Practice.Data;
using Practise.Data.DTO.Auth;
using Practise.Data.DTO.User;
using Swashbuckle.AspNetCore.Annotations;

namespace Practise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public UserController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetUserInfo")]
        [SwaggerOperation(
            Summary = "Получение информации о пользователе",
            Description = "Возвращает информацию о пользователе на основе его идентификатора."
        )]

        public async Task<ActionResult> GetUserInfo([FromQuery] int userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }
            var response = new
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Phone = user.Phone,
                Role = user.Role
            };
            return Ok(response);
        }
        [HttpPut("PutUserInfo")]
        [SwaggerOperation(
            Summary = "меняем номер телефона",
            Description = "меняем номер телефона."
        )]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateUserInfo(int userId, [FromBody] UserDTO dto)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }
            user.Username = dto.Username ?? user.Username;
            user.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? user.Phone : dto.Phone;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
