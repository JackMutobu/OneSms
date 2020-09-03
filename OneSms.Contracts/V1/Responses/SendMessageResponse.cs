namespace OneSms.Contracts.V1.Responses
{
    public class SendMessageResponse
    {
        public int SentMessages { get; set; }

        public int PendingMessages { get; set; }

        public string TransactionId { get; set; }
    }
}
