using System.ComponentModel.DataAnnotations;

namespace OneSms.Web.Shared.Models
{
    public class UssdActionStep:BaseModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Value { get; set; }

        [Required]
        public bool IsPlaceHolder { get; set; }

        public bool CanSkipe { get; set; }

        public int UssdActionId { get; set; }

        public UssdAction UssdAction { get; set; }
    }
}
