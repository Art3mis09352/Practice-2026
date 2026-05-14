using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Data.DTO.Route.Request
{
    public class UpdateRouteMetaDTO
    {
        [MaxLength(200)]
        public string? Title { get; set; }
        [MaxLength(2000)]
        public string? Description { get; set; }
        [MaxLength(32)]
        public string? CoverEmoji { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool? IsPublic { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Бюджет не может быть отрицательным.")]
        public decimal? Budget { get; set; }
    }

}
