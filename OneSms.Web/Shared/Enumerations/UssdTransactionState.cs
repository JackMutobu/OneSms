namespace OneSms.Web.Shared.Enumerations
{
    public enum UssdTransactionState
    {
        Sent,
        Executing,
        Done,
        Canceled,
        Retrying,
        Failed
    }
}
