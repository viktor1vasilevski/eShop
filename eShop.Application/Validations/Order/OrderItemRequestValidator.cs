using eShop.Application.Requests.Order;
using FluentValidation;

namespace eShop.Application.Validations.Order;

public class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product id is required.")
            .Must(id => id != Guid.Empty).WithMessage("Product id must be a valid guid");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");
    }
}
