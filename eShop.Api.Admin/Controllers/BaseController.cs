using eShop.Api.Admin.Responses;
using eShop.Application.Enums;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Admin.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected ActionResult HandleResponse<T>(Result<T> result)
    {
        return result.Status switch
        {
            ResultStatus.NoContent    => NoContent(),
            ResultStatus.NotFound     => NotFound(new ApiResponse<T>     { Message = result.Message }),
            ResultStatus.BadRequest   => BadRequest(new ApiResponse<T>   { Message = result.Message }),
            ResultStatus.Unauthorized => Unauthorized(new ApiResponse<T> { Message = result.Message }),
            ResultStatus.Conflict     => StatusCode(409, new ApiResponse<T> { Message = result.Message }),
            ResultStatus.Error        => StatusCode(500, new ApiResponse<T> { Message = result.Message }),
            ResultStatus.Created      => StatusCode(201, new ApiResponse<T> { Data = result.Data, Message = result.Message }),
            _                         => Ok(new ApiResponse<T> { Data = result.Data, Message = result.Message, TotalCount = result.TotalCount }),
        };
    }
}
