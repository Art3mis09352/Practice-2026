using Application.DTO.Block;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers.OwnerControllers
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

        [HttpPatch("blocks/{id:int}")]
        [SwaggerOperation(
            Summary = "Обновление точки",
            Description = "Обновляет существующую точку/блок."
        )]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status200OK)]
                [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BlockResponseDTO>> UpdateBlock(int id, [FromBody] UpdateBlockRequestDTO dto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }
            var block = await _dbContext.Blocks.FindAsync(id);
            if (block == null || block.OwnerId != ownerId)
            {
                return NotFound();
            }
            block.Title = dto.Title ?? block.Title;
            block.Description = dto.Description ?? block.Description;
            block.Category = dto.Category ?? block.Category;
            block.City = dto.City ?? block.City;
            block.Address = dto.Address ?? block.Address;
            block.Latitude = dto.Latitude ?? block.Latitude;
            block.Longitude = dto.Longitude ?? block.Longitude;
            block.AvgPrice = dto.AvgPrice ?? block.AvgPrice;

            block.IsApproved = false;
            
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
            return Ok(response);
        }

        [HttpDelete("blocks/{id:int}")]
        [SwaggerOperation(
            Summary = "Удаление точки",
            Description = "Удаляет существующую точку/блок."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
                [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBlock(int id)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }
            var block = await _dbContext.Blocks.FindAsync(id);
            if (block == null || block.OwnerId != ownerId)
            {
                return NotFound();
            }
            _dbContext.Blocks.Remove(block);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
