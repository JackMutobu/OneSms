namespace OneSms.Web.Shared.Enumerations
{
    public enum MessageTransactionState
    {
        Sending,
        Sent,
        Delivered,
        Canceled,
        Retrying,
        Failed
    }
}
