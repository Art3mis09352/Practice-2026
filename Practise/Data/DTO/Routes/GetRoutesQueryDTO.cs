// Data/DTO/Route/GetRoutesQueryDTO.cs
namespace Practice.Data.DTO.Route
{
    public class GetRoutesQueryDTO
    {
        public string? City { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
