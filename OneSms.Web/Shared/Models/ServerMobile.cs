using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Web.Shared.Models
{
    public class ServerMobile:BaseModel
    {
        public Guid Key { get; set; }

        [Required]
        public string Name { get; set; }

        public string UserEmail { get; set; }

        public bool IsTimServer { get; set; }

        public ICollection<SimCard> Sims { get; set; }
    }
}
