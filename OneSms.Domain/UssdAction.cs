using OneSms.Contracts.V1.Enumerations;
using System.Collections.Generic;

namespace OneSms.Domain
{
    public class UssdAction
    {
        public int Id { get; set; }

        public UssdAction()
        {
            Steps = new HashSet<UssdActionStep>();
        }

        public string Name { get; set; } = null!;

        public string UssdNumber { get; set; } = null!;

        public NetworkActionType ActionType { get; set; }

        public string KeyLogins { get; set; } = null!;

        public string KeyProblems { get; set; } = null!;

        public int NetworkId { get; set; }

        public NetworkOperator? Network { get; set; }

        public ICollection<UssdActionStep> Steps { get; set; }
    }
}
