﻿using System.ComponentModel.DataAnnotations;

namespace EasyHealth.Models
{
    public class LoginViewModel
    {

        [Required]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        public bool RememberMe { get; set; }
    }
}
