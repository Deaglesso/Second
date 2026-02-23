using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Second.API.Infrastructure.Exceptions;
using Second.Application.Exceptions;

namespace Second.API.ContractTests;

public sealed class ProblemDetailsContractTests
{
    [Fact]
    public async Task TryHandleAsync_AppException_ReturnsStableProblemDetailsShape()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, new TestHostEnvironment());
        var context = CreateHttpContext("/api/v1/chats");

        await handler.TryHandleAsync(
            context,
            new ForbiddenAppException("You cannot send messages as another user.", "sender_mismatch"),
            CancellationToken.None);

        context.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(context.Response.Body);
        var root = json.RootElement;

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Equal(StatusCodes.Status403Forbidden, root.GetProperty("status").GetInt32());
        Assert.True(root.TryGetProperty("title", out _));
        Assert.True(root.TryGetProperty("detail", out _));
        Assert.True(root.TryGetProperty("type", out _));
        Assert.True(root.TryGetProperty("instance", out _));
        Assert.True(root.TryGetProperty("traceId", out _));
        Assert.True(root.TryGetProperty("timestampUtc", out _));
        Assert.Equal("sender_mismatch", root.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_FluentValidationException_IncludesErrorsExtension()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, new TestHostEnvironment());
        var context = CreateHttpContext("/api/v1/auth/register");
        var failures = new[]
        {
            new ValidationFailure("Email", "Email is required."),
            new ValidationFailure("Password", "Password is required.")
        };

        await handler.TryHandleAsync(context, new ValidationException(failures), CancellationToken.None);

        context.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(context.Response.Body);
        var root = json.RootElement;

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("validation_failed", root.GetProperty("errorCode").GetString());
        Assert.True(root.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Email", out _));
        Assert.True(errors.TryGetProperty("Password", out _));
    }

    private static DefaultHttpContext CreateHttpContext(string path)
    {
        return new DefaultHttpContext
        {
            Request = { Path = path },
            Response = { Body = new MemoryStream() }
        };
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Second.API.ContractTests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
