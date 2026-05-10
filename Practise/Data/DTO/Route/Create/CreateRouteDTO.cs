using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.DTO.Route.Create
{
    public class CreateRouteDTO
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
        public decimal? Budget { get; set; }

        public bool IsPublic { get; set; } = false;

        [MinLength(1)]
        public List<CreateRouteDayDTO> Days { get; set; } = new();
    }
}
