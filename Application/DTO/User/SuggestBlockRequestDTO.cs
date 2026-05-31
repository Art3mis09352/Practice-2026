using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Block
{
    public class SuggestBlockRequestDTO
    {
        [Required(ErrorMessage = "Название точки обязательно")]
        [MaxLength(200, ErrorMessage = "Название точки не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов")]
        public string? Description { get; set; }

        [MaxLength(100, ErrorMessage = "Категория не должна превышать 100 символов")]
        public string? Category { get; set; }

        [Required(ErrorMessage = "Город обязателен")]
        [MaxLength(150, ErrorMessage = "Название города не должно превышать 150 символов")]
        public string City { get; set; } = string.Empty;

        [MaxLength(300, ErrorMessage = "Адрес не должен превышать 300 символов")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Широта обязательна")]
        public decimal Latitude { get; set; }

        [Required(ErrorMessage = "Долгота обязательна")]
        public decimal Longitude { get; set; }
    }
}