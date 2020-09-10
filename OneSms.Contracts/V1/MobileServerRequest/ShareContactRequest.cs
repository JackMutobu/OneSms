namespace OneSms.Contracts.V1.MobileServerRequest
{
    public class ShareContactRequest:BaseWhatsappRequest
    {
        public string VcardInfo { get; set; }
    }
}
