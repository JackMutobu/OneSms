namespace OneSms.Contracts.V1.MobileServerRequest
{
    public class ShareContactRequest:BaseMessageRequest
    {
        public string VcardInfo { get; set; }
    }
}
