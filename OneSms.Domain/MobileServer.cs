using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Domain
{
    public class MobileServer
    {
        public MobileServer()
        {
            Sims = new HashSet<SimCard>();
        }
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Secret { get; set; } = null!;

        public ICollection<SimCard> Sims { get; set; }
    }
}
