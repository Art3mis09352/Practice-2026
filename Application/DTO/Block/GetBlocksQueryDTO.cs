namespace Application.DTO.Block
{
    public class GetBlocksQueryDTO
    {
        public string? City { get; set; }
        public string? Category { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
