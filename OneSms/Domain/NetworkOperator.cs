using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Domain
{
    public class NetworkOperator
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Network")]
        public string Name { get; set; } = null!;

        [Required]
        [DisplayName("Alias")]
        public string Alias { get; set; } = null!;
    }
}
