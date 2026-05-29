using Application.Features.Common;
using Infrastructure.Data;
using MediatR;

namespace Application.Features.Admin;

public sealed class DeleteAdminBlockHandler : IRequestHandler<DeleteAdminBlockCommand, FeatureResult>
{
    private readonly AppDbContext _dbContext;

    public DeleteAdminBlockHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FeatureResult> Handle(DeleteAdminBlockCommand request, CancellationToken cancellationToken)
    {
        var block = await _dbContext.Blocks.FindAsync([request.Id], cancellationToken);
        if (block == null)
        {
            return new FeatureNotFound("Точка не найдена.");
        }

        _dbContext.Blocks.Remove(block);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new FeatureNoContent();
    }
}
