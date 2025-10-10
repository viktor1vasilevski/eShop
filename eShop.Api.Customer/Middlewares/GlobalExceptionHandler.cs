using eShop.Application.Enums;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Diagnostics;

namespace eShop.Api.Customer.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var response = new ApiResponse<object>
        {
            Data = null,
            Message = "An unexpected error occurred.",
            Status = ResponseStatus.Error,
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
