using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.DTO.Route.Request
{
    public class UpdateRouteMetaDTO
    {
        [MaxLength(200)]
        public string? Title { get; set; }
        [MaxLength(2000)]
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsPublic { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Бюджет не может быть отрицательным.")]
        public decimal? Budget { get; set; }
    }

}
