// Data/DTO/Block/UpdateBlockRequestDTO.cs
using System.ComponentModel.DataAnnotations;

namespace Practice.Data.DTO.Block
{
    public class UpdateBlockRequestDTO
    {
        [MaxLength(200, ErrorMessage = "Название точки не должно превышать 200 символов")]
        public string? Title { get; set; }

        [MaxLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов")]
        public string? Description { get; set; }

        [MaxLength(100, ErrorMessage = "Категория не должна превышать 100 символов")]
        public string? Category { get; set; }

        [MaxLength(150, ErrorMessage = "Название города не должно превышать 150 символов")]
        public string? City { get; set; }

        [MaxLength(300, ErrorMessage = "Адрес не должен превышать 300 символов")]
        public string? Address { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "Средняя цена должна быть положительной")]
        public decimal? AvgPrice { get; set; }
    }
}
