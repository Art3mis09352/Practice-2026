using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;

namespace Application.DTO.User
{
    public class UpdateUserProfileDTO
    {
        [MaxLength(100)]
        public string? Username { get; set; }

        [Phone(ErrorMessage = "Некорректный формат телефона")]
        public string? Phone { get; set; }
    }
}

