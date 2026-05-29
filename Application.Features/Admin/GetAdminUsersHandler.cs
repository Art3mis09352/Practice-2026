using Application.DTO.Admin;
using Application.DTO.User;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin;

public sealed class GetAdminUsersHandler : IRequestHandler<GetAdminUsersQuery, PagedAdminUsersResponseDTO>
{
    private readonly UserManager<User> _userManager;

    public GetAdminUsersHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<PagedAdminUsersResponseDTO> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var page = dto.Page < 1 ? 1 : dto.Page;
        var pageSize = dto.PageSize < 1 ? 10 : dto.PageSize;
        if (pageSize > 50) pageSize = 50;

        var query = _userManager.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Search))
        {
            var search = dto.Search.Trim().ToLower();
            query = query.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                (u.Email != null && u.Email.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.UserName)
            .ThenBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<UserInfoResponseDTO>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new UserInfoResponseDTO
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                Phone = user.PhoneNumber,
                Roles = roles
            });
        }

        return new PagedAdminUsersResponseDTO
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
