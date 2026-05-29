using Application.DTO.Block;
using Application.Features.Common;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Owner;

public sealed class GetOwnerBlockHandler : IRequestHandler<GetOwnerBlockQuery, FeatureResult>
{
    private readonly AppDbContext _dbContext;

    public GetOwnerBlockHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FeatureResult> Handle(GetOwnerBlockQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerId))
        {
            return new FeatureUnauthorized();
        }

        var block = await _dbContext.Blocks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.OwnerId == request.OwnerId, cancellationToken);

        if (block == null)
        {
            return new FeatureNotFound();
        }

        return new FeatureOkResult<BlockResponseDTO>(new BlockResponseDTO
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
        });
    }
}
