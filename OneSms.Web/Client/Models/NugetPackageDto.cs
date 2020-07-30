using System;

namespace OneSms.Web.Client.Models
{
    public class NugetPackageDto
    {
        public NugetPackageDto()
        { }

        public Uri IconUrl { get; set; }
        public string Description { get; set; }
        public Uri ProjectUrl { get; set; }
        public string Title { get; set; }
    }
}
