using System.Collections.Generic;
using System.Net;

namespace Second.Application.Exceptions
{
    public class ValidationAppException : AppException
    {
        public ValidationAppException(string detail, IReadOnlyDictionary<string, string[]> errors, string errorCode = "validation_failed")
            : base("Validation Failed", detail, HttpStatusCode.BadRequest, errorCode)
        {
            Errors = errors;
        }

        public IReadOnlyDictionary<string, string[]> Errors { get; }
    }
}
