using eShop.Application.Enums;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Admin.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected ActionResult HandleResponsee<T>(ApiResponse<T> response) where T : class
    {
        return response.Status switch
        {
            ResponseStatus.Success => Ok(response),
            ResponseStatus.BadRequest => BadRequest(response),
            ResponseStatus.NotFound => NotFound(response),
            ResponseStatus.Created => StatusCode(201, response),
            ResponseStatus.NoContent => NoContent(),
            ResponseStatus.Error => StatusCode(500, response),
            ResponseStatus.Conflict => StatusCode(409, response),
            ResponseStatus.Unauthorized => Unauthorized(response),
            _ => Ok(response),
        };
    }
}
