using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Practice.Data;
using Practice.Data.DTO.Auth;
using Practice.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.Swagger.Annotations;

namespace Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public LoginController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation(
            Summary = "Авторизация пользователя",
            Description = "Авторизация пользователя и возвращает данные для ответа API."
        )]
        [ProducesResponseType(typeof(ResponseLoginDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ResponseLoginDTO>> Login([FromBody] LoginDTO dto)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || user.Password != dto.Password)
            {
                return Unauthorized("Неверный email или пароль.");
            }

         
            var response = new ResponseLoginDTO
            {
                Email = user.Email,
                Username = user.Username,
                Phone = user.Phone,
                Role = user.Role,
                Token = null
            };

            return Ok(response);
        }
      

    }
}
