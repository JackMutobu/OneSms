using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Web.Shared.Models
{
    public class OneSmsApp:BaseModel
    {
        public OneSmsApp()
        {
            Sims = new Collection<AppSim>();
        }
        [Required]
        public string Name { get; set; }

        [Key]
        public Guid AppId { get; set; }

        public Guid ClientId { get; set; }

        public Guid ClientSecret { get; set; }

        [Required]
        public string UserEmail { get; set; }

        public string SmsBalance { get; set; }

        public string Credit { get; set; }

        public ICollection<AppSim> Sims { get; set; }
    }
}
