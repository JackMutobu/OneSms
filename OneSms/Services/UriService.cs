using OneSms.Contracts.V1;

namespace OneSms.Services
{
    public interface IUriService
    {
        string BaseUri { get; }

        string InternetUrl { get; }

        string GetMessageByTransactionId(string controller, string transactionId);
    }

    public class UriService : IUriService
    {
        public UriService(string baseUri,string internetUrl = "http://afrisofttech-001-site20.btempurl.com")
        {
            BaseUri = baseUri;
            InternetUrl = internetUrl;
        }
        public string BaseUri { get; }

        public string GetMessageByTransactionId(string controller, string transactionId)
            => $"{BaseUri}{ApiRoutes.Base}{controller}/transaction/{transactionId}";

        public string InternetUrl { get; }
    }
}
