using Application.DTO.User;

namespace Application.DTO.Admin
{
    public class PagedAdminUsersResponseDTO
    {
        public IReadOnlyCollection<UserInfoResponseDTO> Items { get; set; } = Array.Empty<UserInfoResponseDTO>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
