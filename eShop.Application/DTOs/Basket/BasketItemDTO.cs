namespace eShop.Application.DTOs.Basket;

public class BasketItemDTO
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public int UnitQuantity { get; set; }
    public string? ImageDataUrl { get; set; }
}
