using eShop.Application.Requests.Category;
using FluentValidation;

namespace eShop.Application.Validations.Category;

public class CreateUpdateCategoryRequestValidator : AbstractValidator<CreateUpdateCategoryRequest>
{
    public CreateUpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.");
    }
}
