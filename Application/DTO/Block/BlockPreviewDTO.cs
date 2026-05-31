// Data/DTO/Block/BlockPreviewDTO.cs
using Domain.Enums;

namespace Application.DTO.Block
{
    public class BlockPreviewDTO
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Category { get; set; }

        public string City { get; set; } = string.Empty;

        public string? Address { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public BlockStatus Status { get; set; }
        public string? PreviewPhotoUrl { get; set; }
        public int PhotosCount { get; set; }
        public string? ModerationComment { get; set; }
    }
}
