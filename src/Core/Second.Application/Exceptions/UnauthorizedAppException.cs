using System.Net;

namespace Second.Application.Exceptions
{
    public class UnauthorizedAppException : AppException
    {
        public UnauthorizedAppException(string detail, string errorCode = "unauthorized")
            : base("Unauthorized", detail, HttpStatusCode.Unauthorized, errorCode)
        {
        }
    }
}
