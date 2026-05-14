// Data/DTO/Route/GetRoutesQueryDTO.cs
namespace Application.Data.DTO.Route.Read
{
    public class GetRoutesQueryDTO
    {
        public string? Search { get; set; }
        public string? City { get; set; }
        public DateOnly? StartDateFrom { get; set; }
        public DateOnly? StartDateTo { get; set; }
        public string? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
