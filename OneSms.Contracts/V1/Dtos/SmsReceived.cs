namespace OneSms.Contracts.V1.Dtos
{
    public class SmsReceived: BaseMessageReceived
    {
        public int SimSlot { get; set; }
    }
}
