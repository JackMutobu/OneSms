namespace OneSms.Domain
{
    public class UssdActionStep
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Value { get; set; } = null!;

        public bool IsPlaceHolder { get; set; }

        public bool CanSkipe { get; set; }

        public int UssdActionId { get; set; }

        public UssdAction? UssdAction { get; set; }
    }
}
