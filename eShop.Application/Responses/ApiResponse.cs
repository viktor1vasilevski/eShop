using eShop.Application.Enums;

namespace eShop.Application.Responses;

public class ApiResponse<T> where T : class
{
    public T? Data { get; set; }
    public string? Message { get; set; }
    public ResponseStatus NotificationType { get; set; }
    public string? Location { get; set; }
    public int? TotalCount { get; set; }
}
