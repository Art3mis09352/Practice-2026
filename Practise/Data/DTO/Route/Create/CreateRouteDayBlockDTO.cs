using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.DTO.Route.Create
{
    public class CreateRouteDayBlockDTO
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int BlockId { get; set; }

        [Range(1, int.MaxValue)]
        public int OrderInDay { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
