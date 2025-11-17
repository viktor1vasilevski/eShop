using eShop.Application.Requests.Customer.Auth;
using FluentValidation;

namespace eShop.Application.Validations.Customer.Auth;

public class CustomerRegisterRequestValidator : AbstractValidator<CustomerRegisterRequest>
{
    private const string PasswordPattern =
        "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$ %^&*-]).{4,}$";

    private const string EmailPattern =
        "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+(\\.[a-zA-Z]{2,})+$";

    public CustomerRegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(2).WithMessage("Username must be at least 2 characters long")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .Matches(EmailPattern).WithMessage("Email format is invalid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .Matches(PasswordPattern)
            .WithMessage("Password must contain at least one uppercase, one lowercase, one number, one special character, and be at least 4 characters long");
    }
}
