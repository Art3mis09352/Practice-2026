using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Data.DTO.Route.Request
{
    public class CreateRouteDayRequestDTO
    {
        public int DayNumber { get; set; }
        public string? Title { get; set; }
        public string? Notes { get; set; }
    }

}
