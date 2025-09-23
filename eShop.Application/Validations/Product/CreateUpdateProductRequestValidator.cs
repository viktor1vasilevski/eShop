
using eShop.Application.Requests.Admin.Product;
using FluentValidation;

namespace eShop.Application.Validations.Product;

public class CreateUpdateProductRequestValidator : AbstractValidator<CreateUpdateProductRequest>
{
    public CreateUpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Subcategory id is required.")
            .Must(id => id != Guid.Empty).WithMessage("Subcategory id must be a valid guid");

        RuleFor(x => x.Price)
            .NotNull().WithMessage("Unit price is required.")
            .GreaterThan(0).WithMessage("Unit price must be greater than zero.");

        RuleFor(x => x.Quantity)
            .NotNull().WithMessage("Unit quantity is required.")
            .GreaterThan(0).WithMessage("Unit quantity must be greater than zero.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2500).WithMessage("Description cannot exceed 2500 characters.");

        RuleFor(x => x.Image)
            .NotEmpty().WithMessage("Product name is required.");
    }
}
