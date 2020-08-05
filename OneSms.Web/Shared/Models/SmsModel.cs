namespace OneSms.Web.Shared.Models
{
    public class SmsModel
    {
        public string Id { get; set; }
        public string MessageBody { get; set; }
        public string OriginatingAddress { get; set; }
        public string ThreadId { get; set; }
        public string Date { get; set; }
        public string Person { get; set; }
        public string Type { get; set; }
    }
}
