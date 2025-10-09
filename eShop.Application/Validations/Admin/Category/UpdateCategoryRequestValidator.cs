using eShop.Application.Requests.Admin.Category;
using FluentValidation;

namespace eShop.Application.Validations.Admin.Category
{
    public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
    {
        public UpdateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.");
        }
    }
}
