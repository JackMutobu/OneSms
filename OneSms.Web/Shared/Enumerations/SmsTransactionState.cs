namespace OneSms.Web.Shared.Enumerations
{
    public enum SmsTransactionState
    {
        Sending,
        Sent,
        Delivered,
        Canceled,
        Retrying
    }
}
