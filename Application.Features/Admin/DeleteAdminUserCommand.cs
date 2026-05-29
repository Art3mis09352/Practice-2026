using Application.Features.Common;
using MediatR;

namespace Application.Features.Admin;

public sealed record DeleteAdminUserCommand(string Id, string? CurrentUserId) : IRequest<FeatureResult>;
