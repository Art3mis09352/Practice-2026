using Application.Features.Common;
using MediatR;

namespace Application.Features.Owner;

public sealed class GetOwnerStatsHandler : IRequestHandler<GetOwnerStatsQuery, FeatureResult>
{
    public Task<FeatureResult> Handle(GetOwnerStatsQuery request, CancellationToken cancellationToken)
    {
        if (request.OwnerId == null || !request.IsOwnerRole)
        {
            return Task.FromResult<FeatureResult>(new FeatureUnauthorized());
        }

        return Task.FromResult<FeatureResult>(new FeatureEmptyOk());
    }
}
