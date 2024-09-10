﻿using System.ComponentModel.DataAnnotations;

namespace IdentificationService.API.DTO
{
    public class UserDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "At least one role is required.")]
        public List<string> Roles { get; set; }
    }
}

