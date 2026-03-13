using eShop.Api.Customer.Responses;
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
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(new ApiResponse<object> { Message = "An unexpected error occurred." }, _jsonOptions.Value.JsonSerializerOptions, cancellationToken);

        return true;
    }
}
