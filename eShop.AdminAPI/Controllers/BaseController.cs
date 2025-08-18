using eShop.Application.Enums;
using eShop.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace eShop.AdminAPI.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResponse<T>(ApiResponse<T> response) where T : class
    {
        return response.NotificationType switch
        {
            ResponseStatus.Success => Ok(response),
            ResponseStatus.BadRequest => BadRequest(response),
            ResponseStatus.NotFound => NotFound(response),
            ResponseStatus.Created => StatusCode(201, response),
            ResponseStatus.NoContent => NoContent(),
            ResponseStatus.ServerError => StatusCode(500, response),
            ResponseStatus.Conflict => StatusCode(409, response),
            ResponseStatus.Unauthorized => Unauthorized(response),
            _ => Ok(response),
        };
    }
}
