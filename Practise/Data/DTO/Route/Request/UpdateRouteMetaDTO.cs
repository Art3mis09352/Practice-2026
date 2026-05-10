using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Data.DTO.Route.Request
{
    public class UpdateRouteMetaDTO
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsPublic { get; set; }
        public decimal? Budget { get; set; }
    }

}
