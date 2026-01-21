using eShop.Application.Enums;
using eShop.Application.Exceptions;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace eShop.Api.Admin.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode;
        string message;
        ResponseStatus responseStatus;

        switch (exception)
        {
            case DomainValidationException ex:
                _logger.LogWarning(ex, "Domain validation failed: {Message}", ex.Message);
                statusCode = StatusCodes.Status400BadRequest;
                message = ex.Message;
                responseStatus = ResponseStatus.BadRequest;
                break;

            case ExternalDependencyException ex:
                _logger.LogError(ex, "External dependency failure");
                statusCode = StatusCodes.Status503ServiceUnavailable;
                message = "Service temporarily unavailable.";
                responseStatus = ResponseStatus.Error;
                break;

            default:
                _logger.LogError(exception, "Unhandled exception");
                statusCode = StatusCodes.Status500InternalServerError;
                message = "An unexpected error occurred.";
                responseStatus = ResponseStatus.Error;
                break;
        }

        var response = new ApiResponse<object>
        {
            Data = null,
            Message = message,
            Status = responseStatus
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
