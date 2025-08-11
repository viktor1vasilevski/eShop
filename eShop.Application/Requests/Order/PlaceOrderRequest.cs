namespace eShop.Application.Requests.Order;

public class PlaceOrderRequest
{
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}
