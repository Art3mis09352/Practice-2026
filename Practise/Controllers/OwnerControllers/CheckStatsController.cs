using Application.Features.Common;
using Application.Features.Owner;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Practice.Controllers.OwnerControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class CheckStatsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CheckStatsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("stats")]
        public async Task<ActionResult> GetStats()
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _mediator.Send(new GetOwnerStatsQuery(ownerId, User.IsInRole("Owner")));
            return result.ToActionResult(this);
        }
    }
}
