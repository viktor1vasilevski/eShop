using eShop.Application.Enums;

namespace eShop.Application.Responses.Shared.Base;

public class ApiResponse<T> where T : class
{
    public T? Data { get; set; }
    public string? Message { get; set; }
    public ResponseStatus Status { get; set; }
    public int? TotalCount { get; set; }
}
