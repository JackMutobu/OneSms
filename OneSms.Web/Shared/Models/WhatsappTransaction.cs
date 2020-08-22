namespace OneSms.Web.Shared.Models
{
    public class WhatsappTransaction: MessageTransaction
    {
        public WhatsappTransaction()
        {
            MessageTransactionProcessor = Enumerations.MessageTransactionProcessor.Whatsapp;
        }

        public string ImageLinkOne { get; set; }

        public string ImageLinkTwo { get; set; }

        public string ImageLinkThree { get; set; }
    }
}
