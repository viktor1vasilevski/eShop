using eShop.Api.Admin.Responses;
using eShop.Application.Exceptions;
using eShop.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace eShop.Api.Admin.Middlewares;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> _logger,
    IOptions<JsonOptions> _jsonOptions) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode;
        string message;

        switch (exception)
        {
            case DomainValidationException ex:
                _logger.LogWarning(ex, "Domain validation failed on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
                statusCode = StatusCodes.Status400BadRequest;
                message = ex.Message;
                break;

            case JwtConfigurationException ex:
                _logger.LogCritical(ex, "JWT configuration is missing or invalid on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
                statusCode = StatusCodes.Status500InternalServerError;
                message = "Authentication service is not configured correctly.";
                break;

            case ExternalDependencyException ex:
                _logger.LogError(ex, "External dependency failure on {Method} {Path}: {ExternalMessage}", httpContext.Request.Method, httpContext.Request.Path, ex.Message);
                statusCode = StatusCodes.Status503ServiceUnavailable;
                message = "Service temporarily unavailable.";
                break;

            default:
                _logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
                statusCode = StatusCodes.Status500InternalServerError;
                message = "An unexpected error occurred.";
                break;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(new ApiResponse<object> { Message = message }, _jsonOptions.Value.JsonSerializerOptions, cancellationToken);

        return true;
    }
}
