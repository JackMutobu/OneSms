using OneSms.Contracts.V1.Dtos;
using System;

namespace OneSms.Droid.Server.Models
{
    public class ImageData
    {
        public DateTime DateTime { get; set; }

        public int Size { get; set; }

        public int Retry { get; set; } = 0;

        public WhastappMessageReceived WhastappMessage { get; set; }
    }
}