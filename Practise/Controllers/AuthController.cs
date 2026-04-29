using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Data;
using Practice.Data.DTO.Auth;
using Practice.Models.Entities;
using Practice.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Practice.Controllers
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

            return CreatedAtAction(nameof(Register), new { id = user.Id }, new ResponseRegisterDTO
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.UserName,
                Phone = user.PhoneNumber,
                Roles = new List<string> { role },
                Token = token
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

            return CreatedAtAction(nameof(Register), new { id = user.Id }, new ResponseRegisterDTO
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.UserName,
                Phone = user.PhoneNumber,
                Roles = new List<string> { role },
                Token = token
            });
        }
    }
}
