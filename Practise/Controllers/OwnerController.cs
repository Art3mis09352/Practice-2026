using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Practice.Data;
using Practice.Data.DTO.Auth;
using Practice.Data.DTO.Block;
using Practice.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class OwnerController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public OwnerController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("blocks")]
        [SwaggerOperation(
            Summary = "Точка",
            Description = "Создает новую точку/блок."
        )]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BlockResponseDTO>> CreateBlock([FromBody] CreateBlockRequestDTO dto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            var block = new Block
            {
                OwnerId = ownerId,
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                City = dto.City,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                AvgPrice = dto.AvgPrice,
                IsApproved = false
            };

            _dbContext.Blocks.Add(block);
            await _dbContext.SaveChangesAsync();

            var response = new BlockResponseDTO
            {
                Id = block.Id,
                OwnerId = block.OwnerId,
                Title = block.Title,
                Description = block.Description,
                Category = block.Category,
                City = block.City,
                Address = block.Address,
                Latitude = block.Latitude,
                Longitude = block.Longitude,
                AvgPrice = block.AvgPrice,
                IsApproved = block.IsApproved
            };

            return CreatedAtAction(nameof(CreateBlock), new { id = block.Id }, response);
        }
    }
}
