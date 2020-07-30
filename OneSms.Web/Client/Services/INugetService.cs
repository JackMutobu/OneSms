using OneSms.Web.Client.Models;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneSms.Web.Client.Services
{
    public interface INugetService
    {
        [Get("/nuget/{term}")]
        Task<IEnumerable<NugetPackageDto>> GetPackages(string term);
    }
}
