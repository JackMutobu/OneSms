using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Domain
{
    public class AppUser : IdentityUser
    {
        [Required]
        public string Fristname { get; set; } = null!;

        [Required]
        public string Lastname { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;

        public DateTime CreatedOn { get; set; }
    }
}
