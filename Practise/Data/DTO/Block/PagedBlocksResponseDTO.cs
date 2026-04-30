namespace Practice.Data.DTO.Block
{
    public class PagedBlocksResponseDTO
    {
        public IReadOnlyCollection<BlockPreviewDTO> Items { get; set; } = Array.Empty<BlockPreviewDTO>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
