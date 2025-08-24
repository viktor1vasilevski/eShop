using eShop.Application.Requests.Order;
using FluentValidation;

namespace eShop.Application.Validations.Order;

public class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.")
            .Must(id => id != Guid.Empty).WithMessage("User id must be a valid guid");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("TotalAmount must be greater than zero.");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items list cannot be null.")
            .NotEmpty().WithMessage("At least one item is required.");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemRequestValidator());
    }
}
