using eShop.Application.Requests.Admin.Category;
using FluentValidation;

namespace eShop.Application.Validations.Admin.Category
{
    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.");

            RuleFor(x => x.Image)
                .NotEmpty().WithMessage("Category image is required");
        }
    }
}
