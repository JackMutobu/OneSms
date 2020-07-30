using System;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Web.Shared.Models
{
    public class ServerMobile:BaseModel
    {
        public Guid Key { get; set; }

        [Required]
        public string Name { get; set; }

        public int SimOneId { get; set; }

        public SimCard SimOne { get; set; }

        public int? SimTwoId { get; set; }

        public SimCard SimTwo { get; set; }

        public string UserEmail { get; set; }


    }
}
