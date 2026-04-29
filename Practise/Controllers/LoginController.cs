using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Practice.Data;
using Practice.Data.DTO.Auth;
using Practice.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.Swagger.Annotations;
using Microsoft.AspNetCore.Identity;
using Practice.Services;

namespace Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtTokenService _jwtTokenService;
        

        public LoginController(UserManager<User> userManager, JwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            
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
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized("Неверный email или пароль.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _jwtTokenService.GenerateTokenAsync(user);

            var response = new ResponseLoginDTO
            {
                Email = user.Email,
                Username = user.UserName,
                Phone = user.PhoneNumber,
                Role = roles,
                Token = token
            };

            return Ok(response);
        }
      

    }
}
