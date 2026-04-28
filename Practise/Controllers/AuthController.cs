using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Data;
using Practise.Data.DTO.Auth;
using Practise.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Practise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AuthController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Регистрация пользователя",
            Description = "Создает нового пользователя и возвращает данные для ответа API."
        )]
        [ProducesResponseType(typeof(ResponseRegisterDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ResponseRegisterDTO>> Register([FromBody] RegisterDto dto)
        {
            

            var emailExists = await _dbContext.Users.AnyAsync(user => user.Email == dto.Email);
            if (emailExists)
            {
                return Conflict("Пользователь с таким email уже существует.");
            }

            var user = new User
            {
                Email = dto.Email,
                Password = dto.Password,
                Username = dto.Username ?? string.Empty,
                Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone,
                Role = dto.Role ?? UserRole.User
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var response = new ResponseRegisterDTO
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Phone = user.Phone,
                Role = user.Role,
                Token = null
            };

            return CreatedAtAction(nameof(Register), new { id = user.Id }, response);
        }
    }
}
