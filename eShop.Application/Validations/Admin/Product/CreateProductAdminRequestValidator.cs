using eShop.Application.Requests.Admin.Product;
using FluentValidation;

namespace eShop.Application.Validations.Admin.Product;

public class CreateProductAdminRequestValidator : AbstractValidator<CreateProductAdminRequest>
{
    public CreateProductAdminRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(50).WithMessage("Product name cannot exceed 50 characters");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required")
            .Must(id => id != Guid.Empty).WithMessage("CategoryId must be a valid GUID");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2500).WithMessage("Description cannot exceed 2500 characters");

        RuleFor(x => x.Image)
            .NotEmpty().WithMessage("Product image is required");
    }
}
