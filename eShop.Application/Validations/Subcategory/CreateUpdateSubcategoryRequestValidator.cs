using eShop.Application.Requests.Subcategory;
using FluentValidation;

namespace eShop.Application.Validations.Subcategory;

public class CreateUpdateSubcategoryRequestValidator : AbstractValidator<CreateUpdateSubcategoryRequest>
{
    public CreateUpdateSubcategoryRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .Must(id => id != Guid.Empty)
            .WithMessage("Category id must be a valid guid");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Subcategory name is required.")
            .MinimumLength(2).WithMessage("Subcategory name must be at least 2 characters long.");
    }
}
