using eShop.Application.Requests.Customer.Comment;
using FluentValidation;

namespace eShop.Application.Validations.Customer.Comment;

public class CreateCommentCustomerRequestValidator : AbstractValidator<CreateCommentCustomerRequest>
{
    public CreateCommentCustomerRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .Must(id => id != Guid.Empty)
            .WithMessage("ProductId must be a valid GUID");

        RuleFor(x => x.CommentText)
            .NotEmpty().WithMessage("Comment text is required")
            .MaximumLength(2500).WithMessage("Comment text cannot exceed 2500 characters");

        RuleFor(x => x.Rating)
            .IsInEnum().WithMessage("Invalid rating value");
    }
}
