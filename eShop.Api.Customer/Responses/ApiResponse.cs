namespace eShop.Api.Customer.Responses;

internal sealed class ApiResponse<T>
{
    public T? Data { get; init; }
    public string? Message { get; init; }
    public int? TotalCount { get; init; }
}
