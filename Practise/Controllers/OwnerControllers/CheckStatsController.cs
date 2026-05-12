using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Practice.Controllers.OwnerControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class CheckStatsController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public CheckStatsController(AppDbContext dbContext)
        {
            _appDbContext = dbContext;
        }

        [HttpGet("stats")]
        public async Task<ActionResult> GetStats()
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ownerId == null || !User.IsInRole("Owner"))
            {
                return Unauthorized();
            }

            // Your logic to get stats goes here

            return Ok();
        }
    }
}
