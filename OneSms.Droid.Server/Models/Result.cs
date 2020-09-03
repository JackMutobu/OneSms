using System.Net;

namespace OneSms.Droid.Server.Models
{
    public class Result<T> where T : class
    {
        public Result(HttpStatusCode httpStatusCode, string errorMessage)
        {
            HttpStatusCode = httpStatusCode;
            Message = errorMessage;
            IsSuccess = false;

        }

        public Result(HttpStatusCode httpStatusCode, T value, string message = null)
        {
            HttpStatusCode = httpStatusCode;
            Message = message;
            Value = value;
            IsSuccess = true;
        }

        public HttpStatusCode HttpStatusCode { get; }

        public string Message { get; }

        public T Value { get; }

        public bool IsSuccess { get; }

    }
}