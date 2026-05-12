using Application.Data.DTO.Auth;
using Application.DTO.Auth;
using Domain.Entities;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Swashbuckle.AspNetCore.Annotations;

namespace Practice.Controllers.UnauthorizedControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, JwtTokenService jwtTokenService, IConfiguration configuration)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
        }

        private void SetAuthCookie(string jwt)
        {
            Response.Cookies.Append("auth", jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddHours(12),
                Path = "/"
            });
        }

        [HttpGet("antiforgery")]
        [AllowAnonymous]
        public IActionResult Antiforgery([FromServices] IAntiforgery antiforgery)
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);

            Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            return NoContent();
        }


        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Регистрация пользователя",
            Description = "Создает нового пользователя и с ролью User."
        )]
        [ProducesResponseType(typeof(ResponseRegisterDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ResponseRegisterDTO>> Register([FromBody] RegisterUserDto dto)
        {
            return await RegisterInternalAsync(dto, "User");


        }
        private async Task<ActionResult<ResponseRegisterDTO>> RegisterInternalAsync(RegisterUserDto dto, string role)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                return Conflict("Пользователь с таким email уже существует.");
            }

            var user = new User
            {
                Email = dto.Email,
                UserName = string.IsNullOrEmpty(dto.Username) ? dto.Email : dto.Username,
                PhoneNumber = dto.Phone,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!addRoleResult.Succeeded)
            {
                return BadRequest(addRoleResult.Errors.Select(x => x.Description));
            }

            var token = await _jwtTokenService.GenerateTokenAsync(user);
            SetAuthCookie(token);

            return CreatedAtAction(nameof(Register), new { id = user.Id }, new ResponseRegisterDTO
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.UserName,
                Phone = user.PhoneNumber,
                Roles = new List<string> { role },
                
            });
        }


        [HttpPost("register-owner")]
        [SwaggerOperation(
            Summary = "Регистрация владельца",
            Description = "Создает нового пользователя с ролью Owner."
        )]
        [ProducesResponseType(typeof(ResponseRegisterDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ResponseRegisterDTO>> RegisterOwner(
            [FromBody] RegisterOwnerDTO dto,
            [FromQuery] string inviteToken)
        {
            var ownerInviteToken = _configuration["OwnerRegistration:InviteToken"];

            if (string.IsNullOrWhiteSpace(inviteToken) || inviteToken != ownerInviteToken)
            {
                return Forbid();
            }

            return await RegisterInternalAsync(dto, "Owner");
        }



        private async Task<ActionResult<ResponseRegisterDTO>> RegisterInternalAsync(RegisterOwnerDTO dto, string role)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                return Conflict("Пользователь с таким email уже существует.");
            }

            var user = new User
            {
                Email = dto.Email,
                UserName = string.IsNullOrEmpty(dto.Username) ? dto.Email : dto.Username,
                PhoneNumber = dto.Phone,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, "Owner");
            if (!addRoleResult.Succeeded)
            {
                return BadRequest(addRoleResult.Errors.Select(x => x.Description));
            }

            var token = await _jwtTokenService.GenerateTokenAsync(user);
            SetAuthCookie(token);
            return CreatedAtAction(nameof(Register), new { id = user.Id }, new ResponseRegisterDTO
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.UserName,
                Phone = user.PhoneNumber,
                Roles = new List<string> { role },
                
            });
        }
        [HttpPost("logout")]
        [SwaggerOperation(
            Summary = "logout",
            Description = "Выход пользователя из системы."
        )]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("auth", new CookieOptions
            {
                Path = "/",
                Secure = true,
                SameSite = SameSiteMode.None
            });

            return NoContent();
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
            SetAuthCookie(token);

            var response = new ResponseLoginDTO
            {
                Email = user.Email,
                Username = user.UserName,
                Phone = user.PhoneNumber,
                Role = roles,

            };

            return Ok(response);
        }
    }
}
