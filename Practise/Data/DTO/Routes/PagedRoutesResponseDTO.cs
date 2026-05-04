// Data/DTO/Route/PagedRoutesResponseDTO.cs
namespace Practice.Data.DTO.Route
{
    public class PagedRoutesResponseDTO
    {
        public IReadOnlyCollection<RoutePreviewDTO> Items { get; set; } = Array.Empty<RoutePreviewDTO>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
