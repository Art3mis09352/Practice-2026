using Domain.Enums;

namespace Application.DTO.Admin
{
    public class GetAdminBlocksQueryDTO
    {
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public BlockStatus? Status { get; set; }
    }
}
