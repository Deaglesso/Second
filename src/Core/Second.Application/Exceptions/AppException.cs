using System;
using System.Net;

namespace Second.Application.Exceptions
{
    public abstract class AppException : Exception
    {
        protected AppException(
            string title,
            string detail,
            HttpStatusCode statusCode,
            string errorCode,
            Exception? innerException = null)
            : base(detail, innerException)
        {
            Title = title;
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public string Title { get; }

        public HttpStatusCode StatusCode { get; }

        public string ErrorCode { get; }
    }
}
