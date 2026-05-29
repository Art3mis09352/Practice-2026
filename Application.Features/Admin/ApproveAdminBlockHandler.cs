using Application.Features.Common;
using Infrastructure.Data;
using MediatR;

namespace Application.Features.Admin;

public sealed class ApproveAdminBlockHandler : IRequestHandler<ApproveAdminBlockCommand, FeatureResult>
{
    private readonly AppDbContext _dbContext;

    public ApproveAdminBlockHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FeatureResult> Handle(ApproveAdminBlockCommand request, CancellationToken cancellationToken)
    {
        var block = await _dbContext.Blocks.FindAsync([request.Id], cancellationToken);
        if (block == null)
        {
            return new FeatureNotFound("Точка не найдена.");
        }

        if (!block.IsApproved)
        {
            block.IsApproved = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new FeatureNoContent();
    }
}
