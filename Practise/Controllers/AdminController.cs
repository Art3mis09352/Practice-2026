using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Practice.Data;
using Practise.Data.DTO.Auth;
using Practise.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Practise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AdminController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPatch("ChangeRole")]
        [SwaggerOperation(
            Summary = "Смена роли",
            Description = "Админ меняет роль"
        )]
        [ProducesResponseType(typeof(ResponseRegisterDTO), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> ChangeUserRole(int userId, string newRole)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }
            if (!Enum.TryParse(newRole, true, out UserRole parsedRole))
            {
                return BadRequest("Недопустимая роль.");
            }
            user.Role = parsedRole;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("DeleteUser")]
        [SwaggerOperation(
            Summary = "удалить пользователя",
            Description = "Админ удаляет пользователя"
        )]
        [ProducesResponseType(typeof(ResponseRegisterDTO), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteUser(int userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            return NoContent();

        }
    }
}
