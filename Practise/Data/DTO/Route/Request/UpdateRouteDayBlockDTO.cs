using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Data.DTO.Route.Request
{
    public class UpdateRouteDayBlockDTO
    {
        public int? BlockId { get; set; }
        public int? OrderInDay { get; set; }
        public string? Notes { get; set; }
    }

}
