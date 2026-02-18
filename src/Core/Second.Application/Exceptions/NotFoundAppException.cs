using System.Net;

namespace Second.Application.Exceptions
{
    public class NotFoundAppException : AppException
    {
        public NotFoundAppException(string detail, string errorCode = "not_found")
            : base("Resource Not Found", detail, HttpStatusCode.NotFound, errorCode)
        {
        }
    }
}
