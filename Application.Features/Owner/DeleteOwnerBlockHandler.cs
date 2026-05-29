using Application.Features.Common;
using Infrastructure.Data;
using MediatR;

namespace Application.Features.Owner;

public sealed class DeleteOwnerBlockHandler : IRequestHandler<DeleteOwnerBlockCommand, FeatureResult>
{
    private readonly AppDbContext _dbContext;

    public DeleteOwnerBlockHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FeatureResult> Handle(DeleteOwnerBlockCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerId))
        {
            return new FeatureUnauthorized();
        }

        var block = await _dbContext.Blocks.FindAsync([request.Id], cancellationToken);
        if (block == null || block.OwnerId != request.OwnerId)
        {
            return new FeatureNotFound();
        }

        _dbContext.Blocks.Remove(block);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new FeatureNoContent();
    }
}
