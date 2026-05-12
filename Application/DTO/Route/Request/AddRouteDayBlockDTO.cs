using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Data.DTO.Route.Request
{
    public class AddRouteDayBlockDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "BlockId должен быть больше 0.")]
        public int BlockId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "OrderInDay должен быть больше 0.")]
        public int OrderInDay { get; set; }
        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

}
