using Application.DTO.Block;
using Application.Features.Common;
using Infrastructure.Data;
using MediatR;

namespace Application.Features.Owner;

public sealed class UpdateOwnerBlockHandler : IRequestHandler<UpdateOwnerBlockCommand, FeatureResult>
{
    private readonly AppDbContext _dbContext;

    public UpdateOwnerBlockHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FeatureResult> Handle(UpdateOwnerBlockCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerId))
        {
            return new FeatureUnauthorized();
        }

        var dto = request.Dto;
        var block = await _dbContext.Blocks.FindAsync([request.Id], cancellationToken);
        if (block == null || block.OwnerId != request.OwnerId)
        {
            return new FeatureNotFound();
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

        return new FeatureOkResult<BlockResponseDTO>(response);
    }
}
