using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OneSms.Web.Shared.Models
{
    public class ServerMobile:BaseModel
    {
        public Guid Key { get; set; }

        [Required]
        public string Name { get; set; }

        public string UserEmail { get; set; }

        public ICollection<SimCard> Sims { get; set; }
    }
}
