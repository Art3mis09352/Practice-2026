// Data/DTO/Block/BlockResponseDTO.cs
using Domain.Enums;

namespace Application.DTO.Block
{
    public class BlockResponseDTO
    {
        public int Id { get; set; }

        public string OwnerId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Category { get; set; }

        public string City { get; set; } = string.Empty;

        public string? Address { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public decimal? AvgPrice { get; set; }
        public int? PreviewPhotoId { get; set; }
        public string? PreviewPhotoUrl { get; set; }

        public string? ModerationComment { get; set; }

        public DateTime? ModeratedAt { get; set; }

        public string? ModeratedByUserId { get; set; }

        public BlockStatus Status { get; set; }
        public IReadOnlyCollection<BlockPhotoDTO> Photos { get; set; } = Array.Empty<BlockPhotoDTO>();
        public IReadOnlyCollection<BlockDocumentDTO> Documents { get; set; } = Array.Empty<BlockDocumentDTO>();
    }
}
