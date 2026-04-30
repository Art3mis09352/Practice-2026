using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Data;
using Practice.Data.DTO.Auth;
using Practice.Data.DTO.User;
using Practice.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles="Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _dbContext;

        public AdminController(UserManager<User> userManager, AppDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        
        [HttpDelete("DeleteUser")]
        [SwaggerOperation(
            Summary = "удалить пользователя",
            Description = "Админ удаляет пользователя"
        )]
        [ProducesResponseType(typeof(ResponseRegisterDTO), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Не удалось удалить пользователя.");
            }
            return NoContent();

        }

        [HttpPatch("blocks/{id}/approve")]
        public async Task<ActionResult> ApproveBlock(int id)
        {
            var block = await _dbContext.Blocks.FindAsync(id);
            if (block == null)
            {
                return NotFound("Точка не найдена.");
            }

            block.IsApproved = true;
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }


    }
}
