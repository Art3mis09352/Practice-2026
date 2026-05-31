using System.ComponentModel.DataAnnotations;

namespace Application.Data.DTO.Route.Request
{
    public class UpdateRouteDayCustomPointDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "OrderInDay должен быть больше 0.")]
        public int? OrderInDay { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(120)]
        public string? City { get; set; }

        [MaxLength(120)]
        public string? Category { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }
    }
}