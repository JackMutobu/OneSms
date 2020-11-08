using OneSms.Contracts.V1.Enumerations;

namespace OneSms.Domain
{
    public class NetworkMessageExtractor
    {
        public int Id { get; set; }

        public string? Description { get; set; }

        public string RegexPatern { get; set; } = null!;

        public string OriginatingAddress { get; set; } = null!;

        public int NetworkId { get; set; }

        public NetworkOperator? Network { get; set; }

        public NetworkActionType NetworkAction { get; set; }
    }
}
