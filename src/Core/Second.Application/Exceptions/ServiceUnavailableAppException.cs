using System.Net;

namespace Second.Application.Exceptions
{
    public class ServiceUnavailableAppException : AppException
    {
        public ServiceUnavailableAppException(string detail, string errorCode = "service_unavailable")
            : base("Service Unavailable", detail, HttpStatusCode.ServiceUnavailable, errorCode)
        {
        }
    }
}
