using eShop.Application.Enums;
using eShop.Application.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace eShop.PublicAPI.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var response = new ApiResponse<object>
        {
            Data = null,
            Message = "An unexpected error occurred.",
            NotificationType = ResponseStatus.ServerError,
            Location = null
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
