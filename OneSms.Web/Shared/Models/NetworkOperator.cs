using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Web.Shared.Models
{
    public class NetworkOperator:BaseModel
    {
        [Required]
        [DisplayName("Network")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Alias")]
        public string Alias { get; set; }
    }
}
