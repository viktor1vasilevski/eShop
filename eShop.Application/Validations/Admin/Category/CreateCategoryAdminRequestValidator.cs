using eShop.Application.Requests.Admin.Category;
using FluentValidation;

namespace eShop.Application.Validations.Admin.Category
{
    public class CreateCategoryAdminRequestValidator : AbstractValidator<CreateCategoryAdminRequest>
    {
        public CreateCategoryAdminRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name cannot be empty.")
                .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.")
                .MaximumLength(200).WithMessage("Category name cannot exceed 200 characters.");

            RuleFor(x => x.Image)
                .NotEmpty().WithMessage("Category image is required");
        }
    }
}
