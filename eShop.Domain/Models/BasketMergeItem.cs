using eShop.Domain.Entities;

namespace eShop.Domain.Models;

public record BasketMergeItem(Product Product, int Quantity);
