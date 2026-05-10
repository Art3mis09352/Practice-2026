using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.DTO.Route.Create
{
    public class CreateRouteDayDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "DayNumber должен быть больше 0.")]
        public int DayNumber { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
        [Required]
        public List<CreateRouteDayBlockDTO> Blocks { get; set; } = new();
    }
}
