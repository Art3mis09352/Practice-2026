using Application.DTO.Auth;
using Application.Features.Common;
using MediatR;

namespace Application.Features.Auth;

public sealed record LoginCommand(LoginDTO Dto) : IRequest<FeatureResult>;
