using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.DTO.Route.Request
{
    public class CreateRouteDayRequestDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "DayNumber должен быть больше 0.")]
        public int DayNumber { get; set; }
        [MaxLength(200)]
        public string? Title { get; set; }
        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

}
