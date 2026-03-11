using eShop.Application.Enums;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Admin.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected ActionResult HandleResponse<T>(Result<T> result) where T : class
    {
        var response = new ApiResponse<T>
        {
            Data = result.Data,
            Message = result.Message,
            TotalCount = result.TotalCount,
            Status = MapStatus(result.Status)
        };

        return result.Status switch
        {
            ResultStatus.Success => Ok(response),
            ResultStatus.BadRequest => BadRequest(response),
            ResultStatus.NotFound => NotFound(response),
            ResultStatus.Created => StatusCode(201, response),
            ResultStatus.NoContent => NoContent(),
            ResultStatus.Error => StatusCode(500, response),
            ResultStatus.Conflict => StatusCode(409, response),
            ResultStatus.Unauthorized => Unauthorized(response),
            _ => Ok(response),
        };
    }

    private static ResponseStatus MapStatus(ResultStatus status) => status switch
    {
        ResultStatus.Success => ResponseStatus.Success,
        ResultStatus.Created => ResponseStatus.Created,
        ResultStatus.NotFound => ResponseStatus.NotFound,
        ResultStatus.Conflict => ResponseStatus.Conflict,
        ResultStatus.BadRequest => ResponseStatus.BadRequest,
        ResultStatus.Unauthorized => ResponseStatus.Unauthorized,
        ResultStatus.Error => ResponseStatus.Error,
        ResultStatus.NoContent => ResponseStatus.NoContent,
        _ => ResponseStatus.Success
    };
}
