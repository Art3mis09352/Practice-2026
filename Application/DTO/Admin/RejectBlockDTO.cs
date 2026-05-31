using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Admin
{
    public class RejectBlockDTO
    {
        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;
    }
}