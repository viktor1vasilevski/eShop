using eShop.Application.Enums;
using eShop.Application.Responses.Shared.Base;
using eShop.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace eShop.Api.Admin.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        int statusCode = StatusCodes.Status500InternalServerError;
        string message = "An unexpected error occurred.";

        switch (exception)
        {
            case ExternalDependencyException ex:
                statusCode = StatusCodes.Status503ServiceUnavailable;
                message = ex.Message;
                break;
        }

        var response = new ApiResponse<object>
        {
            Data = null,
            Message = message,
            Status = ResponseStatus.Error,
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
