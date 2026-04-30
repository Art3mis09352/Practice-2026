// Data/DTO/Block/BlockPreviewDTO.cs
namespace Practice.Data.DTO.Block
{
    public class BlockPreviewDTO
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Category { get; set; }

        public string City { get; set; } = string.Empty;

        public string? Address { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public bool IsApproved { get; set; }
    }
}
