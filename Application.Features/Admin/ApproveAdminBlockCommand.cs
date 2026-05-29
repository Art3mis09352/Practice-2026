using Application.Features.Common;
using MediatR;

namespace Application.Features.Admin;

public sealed record ApproveAdminBlockCommand(int Id) : IRequest<FeatureResult>;
