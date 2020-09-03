using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Models
{
    public class OneSmsUser : IdentityUser
    {
        [Required]
        public string Fristname { get; set; }

        [Required]
        public string Lastname { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
