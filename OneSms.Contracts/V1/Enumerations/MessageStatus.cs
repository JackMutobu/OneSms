namespace OneSms.Contracts.V1.Enumerations
{
    public enum MessageStatus
    {
        Sending,
        Sent,
        Delivered,
        Canceled,
        Retrying,
        Failed,
        Executing,
        Pending,
        NumberNotFound
    }
}
