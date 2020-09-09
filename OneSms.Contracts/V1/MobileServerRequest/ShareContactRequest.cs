namespace OneSms.Contracts.V1.MobileServerRequest
{
    public class ShareContactRequest
    {
        public string VcardInfo { get; set; }

        public string Message { get; set; }

        public string Number { get; set; }
    }
}
