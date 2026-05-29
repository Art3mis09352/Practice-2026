using Application.Features.Common;
using MediatR;

namespace Application.Features.Owner;

public sealed record DeleteOwnerBlockCommand(int Id, string? OwnerId) : IRequest<FeatureResult>;
