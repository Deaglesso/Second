using System.Net;

namespace Second.Application.Exceptions
{
    public class ConfigurationAppException : AppException
    {
        public ConfigurationAppException(string detail, string errorCode = "configuration_error")
            : base("Configuration Error", detail, HttpStatusCode.InternalServerError, errorCode)
        {
        }
    }
}
