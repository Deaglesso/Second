using System.Net;

namespace Second.Application.Exceptions
{
    public class ConflictAppException : AppException
    {
        public ConflictAppException(string detail, string errorCode = "conflict")
            : base("Conflict", detail, HttpStatusCode.Conflict, errorCode)
        {
        }
    }
}
