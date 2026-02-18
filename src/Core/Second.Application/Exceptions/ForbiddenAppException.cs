using System.Net;

namespace Second.Application.Exceptions
{
    public class ForbiddenAppException : AppException
    {
        public ForbiddenAppException(string detail, string errorCode = "forbidden")
            : base("Forbidden", detail, HttpStatusCode.Forbidden, errorCode)
        {
        }
    }
}
