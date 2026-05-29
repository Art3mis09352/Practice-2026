using Application.DTO.Block;
using Application.Features.Common;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Unauthorized;

public sealed class GetPublicPointHandler : IRequestHandler<GetPublicPointQuery, FeatureResult>
{
    private readonly AppDbContext _appDbContext;

    public GetPublicPointHandler(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<FeatureResult> Handle(GetPublicPointQuery request, CancellationToken cancellationToken)
    {
        var point = await _appDbContext.Blocks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsApproved, cancellationToken);

        if (point == null)
        {
            return new FeatureNotFound();
        }

        var result = new BlockResponseDTO
        {
            Id = point.Id,
            OwnerId = point.OwnerId,
            Title = point.Title,
            Description = point.Description,
            Category = point.Category,
            City = point.City,
            Address = point.Address,
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            AvgPrice = point.AvgPrice,
            IsApproved = point.IsApproved
        };

        return new FeatureOkResult<BlockResponseDTO>(result);
    }
}
