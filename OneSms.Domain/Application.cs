using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Domain
{
    public class Application
    {
        public Application()
        {
            Sims = new Collection<ApplicationSim>();
            Contacts = new Collection<AppContact>();
        }
        public Guid Id { get; set; }

        public Guid Secret { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? Organization { get; set; }

        public decimal Credit { get; set; }

        public DateTime CreatedOn { get; set; }

        public string UserId { get; set; } = null!;

        public AppUser? User { get; set; }

        public virtual ICollection<ApplicationSim> Sims { get; set; }

        public virtual ICollection<AppContact> Contacts { get; set; }
    }
}
