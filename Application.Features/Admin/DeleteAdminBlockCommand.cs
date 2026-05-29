using Application.Features.Common;
using MediatR;

namespace Application.Features.Admin;

public sealed record DeleteAdminBlockCommand(int Id) : IRequest<FeatureResult>;
