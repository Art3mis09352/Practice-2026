using Application.DTO.Block;
using Application.Features.Common;
using Domain.Entities;
using Infrastructure.Data;
using MediatR;

namespace Application.Features.Owner;

public sealed class CreateOwnerBlockHandler : IRequestHandler<CreateOwnerBlockCommand, FeatureResult>
{
    private readonly AppDbContext _dbContext;

    public CreateOwnerBlockHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FeatureResult> Handle(CreateOwnerBlockCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerId))
        {
            return new FeatureUnauthorized();
        }

        var dto = request.Dto;
        var block = new Block
        {
            OwnerId = request.OwnerId,
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
        await _dbContext.SaveChangesAsync(cancellationToken);

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

        return new FeatureCreatedResult<BlockResponseDTO>(response, "CreateBlock", new { id = block.Id });
    }
}
