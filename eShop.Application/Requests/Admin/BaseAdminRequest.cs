namespace eShop.Application.Requests.Admin;

public class BaseAdminRequest : BaseRequest
{
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
