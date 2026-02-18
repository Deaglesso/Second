using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Second.Application.Exceptions;

namespace Second.API.Infrastructure.Exceptions
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private const string GenericErrorMessage = "An unexpected error occurred while processing the request.";

        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, title, detail, errorCode, errors) = MapException(exception);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Type = $"https://httpstatuses.com/{statusCode}",
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            problemDetails.Extensions["timestampUtc"] = DateTime.UtcNow;
            problemDetails.Extensions["errorCode"] = errorCode;

            if (errors is not null)
            {
                problemDetails.Extensions["errors"] = errors;
            }

            LogException(exception, statusCode, errorCode, httpContext);

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        private (int StatusCode, string Title, string Detail, string ErrorCode, IReadOnlyDictionary<string, string[]>? Errors) MapException(Exception exception)
        {
            return exception switch
            {
                ValidationAppException validationException => (
                    (int)validationException.StatusCode,
                    validationException.Title,
                    validationException.Message,
                    validationException.ErrorCode,
                    validationException.Errors),
                AppException appException => (
                    (int)appException.StatusCode,
                    appException.Title,
                    appException.Message,
                    appException.ErrorCode,
                    null),
                ValidationException fluentValidationException => (
                    StatusCodes.Status400BadRequest,
                    "Validation Failed",
                    "One or more validation errors occurred.",
                    "validation_failed",
                    CreateValidationErrorsDictionary(fluentValidationException)),
                BadHttpRequestException badHttpRequestException => (
                    badHttpRequestException.StatusCode,
                    "Bad Request",
                    badHttpRequestException.Message,
                    "bad_http_request",
                    null),
                JsonException _ => (
                    StatusCodes.Status400BadRequest,
                    "Invalid JSON",
                    "The request payload contains malformed JSON.",
                    "invalid_json",
                    null),
                OperationCanceledException _ => (
                    499,
                    "Request Cancelled",
                    "The request was cancelled by the client.",
                    "request_cancelled",
                    null),
                UnauthorizedAccessException unauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    unauthorizedAccessException.Message,
                    "unauthorized",
                    null),
                ArgumentException argumentException => (
                    StatusCodes.Status400BadRequest,
                    "Bad Request",
                    argumentException.Message,
                    "argument_error",
                    null),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    _hostEnvironment.IsDevelopment() ? exception.Message : GenericErrorMessage,
                    "internal_server_error",
                    null)
            };
        }

        private static IReadOnlyDictionary<string, string[]> CreateValidationErrorsDictionary(ValidationException exception)
        {
            return exception.Errors
                .GroupBy(error => error.PropertyName, error => error.ErrorMessage)
                .ToDictionary(group => group.Key, group => group.Distinct().ToArray());
        }

        private void LogException(Exception exception, int statusCode, string errorCode, HttpContext context)
        {
            var logLevel = statusCode >= StatusCodes.Status500InternalServerError
                ? LogLevel.Error
                : LogLevel.Warning;

            _logger.Log(logLevel, exception,
                "Request {Method} {Path} failed with status {StatusCode} and error code {ErrorCode}. TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                statusCode,
                errorCode,
                context.TraceIdentifier);
        }
    }
}
