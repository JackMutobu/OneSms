namespace OneSms.Domain
{
    public class WhatsappMessage: BaseMessage
    {
        public WhatsappMessage()
        {
            MessageProcessor = Contracts.V1.Enumerations.MessageProcessor.Whatsapp;
        }

        public string? ImageLinkOne { get; set; }

        public string? ImageLinkTwo { get; set; }

        public string? ImageLinkThree { get; set; }
    }
}
