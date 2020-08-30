using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Domain
{
    public class AppUser : IdentityUser
    {
        [Required]
        public string Fristname { get; set; }

        [Required]
        public string Lastname { get; set; }

        [Required]
        public string Role { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
