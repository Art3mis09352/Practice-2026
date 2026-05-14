using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTO.Route.Create
{
    public class CreateRouteDTO
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(32)]
        public string? CoverEmoji { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Бюджет не может быть отрицательным.")]
        public decimal? Budget { get; set; }

        public bool IsPublic { get; set; } = false;
        [Required]
        [MinLength(1, ErrorMessage = "Маршрут должен содержать хотя бы один день.")]
        public List<CreateRouteDayDTO> Days { get; set; } = new();
    }
}
