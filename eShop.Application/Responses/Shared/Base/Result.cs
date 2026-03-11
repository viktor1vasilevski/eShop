using eShop.Application.Enums;

namespace eShop.Application.Responses.Shared.Base;

public class Result<T>
{
    public T? Data { get; private init; }
    public string? Message { get; private init; }
    public ResultStatus Status { get; private init; }
    public int? TotalCount { get; private init; }

    private Result() { }

    public static Result<T> Success(T data, int? totalCount = null, string? message = null) =>
        new() { Status = ResultStatus.Success, Data = data, TotalCount = totalCount, Message = message };

    public static Result<T> Created(T data, string? message = null) =>
        new() { Status = ResultStatus.Created, Data = data, Message = message };

    public static Result<T> NotFound(string? message = null) =>
        new() { Status = ResultStatus.NotFound, Message = message };

    public static Result<T> Conflict(string? message = null) =>
        new() { Status = ResultStatus.Conflict, Message = message };

    public static Result<T> BadRequest(string? message = null) =>
        new() { Status = ResultStatus.BadRequest, Message = message };

    public static Result<T> Unauthorized(string? message = null) =>
        new() { Status = ResultStatus.Unauthorized, Message = message };

    public static Result<T> Error(string? message = null) =>
        new() { Status = ResultStatus.Error, Message = message };

    public static Result<T> NoContent(string? message = null) =>
        new() { Status = ResultStatus.NoContent, Message = message };
}
