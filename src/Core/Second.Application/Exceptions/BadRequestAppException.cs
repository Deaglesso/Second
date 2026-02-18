using System.Net;

namespace Second.Application.Exceptions
{
    public class BadRequestAppException : AppException
    {
        public BadRequestAppException(string detail, string errorCode = "bad_request")
            : base("Bad Request", detail, HttpStatusCode.BadRequest, errorCode)
        {
        }
    }
}
