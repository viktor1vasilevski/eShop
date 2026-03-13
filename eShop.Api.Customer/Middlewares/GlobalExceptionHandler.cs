using eShop.Api.Customer.Responses;
using eShop.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace eShop.Api.Customer.Middlewares;

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
            case JwtConfigurationException ex:
                _logger.LogCritical(ex, "JWT configuration is missing or invalid");
                statusCode = StatusCodes.Status500InternalServerError;
                message = "Authentication service is not configured correctly.";
                break;

            default:
                _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
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
