// Data/DTO/Block/BlockResponseDTO.cs
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

        public bool IsApproved { get; set; }
        public IReadOnlyCollection<BlockPhotoDTO> Photos { get; set; } = Array.Empty<BlockPhotoDTO>();
    }
}
