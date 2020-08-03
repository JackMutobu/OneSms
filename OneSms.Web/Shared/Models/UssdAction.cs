using OneSms.Web.Shared.Enumerations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Web.Shared.Models
{
    public class UssdAction:BaseModel
    {
        public UssdAction()
        {
            Steps = new HashSet<UssdActionStep>();
        }
        [Required]
        public string Name { get; set; }
        [Required]
        public string UssdNumber { get; set; }

        public UssdActionType ActionType { get; set; }


        public string KeyLogins { get; set; }

        public string KeyProblems { get; set; }

        public int NetworkId { get; set; }

        public NetworkOperator Network { get; set; }

        public ICollection<UssdActionStep> Steps { get; set; }
    }
}
