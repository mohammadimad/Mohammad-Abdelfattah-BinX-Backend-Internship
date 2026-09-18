using CardiacMonitor.DTOs;
using FluentValidation;

namespace CardiacMonitor.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    // Defines identity and patient-profile rules for safe public registration.
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(6)
            .Matches("[a-z]")
            .WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]")
            .WithMessage("Password must contain a digit.");
        RuleFor(request => request.FirstName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
        RuleFor(request => request.LastName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
        RuleFor(request => request.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today);
        RuleFor(request => request.Gender)
            .Must(gender =>
                gender != null &&
                (gender.Equals("Male", StringComparison.OrdinalIgnoreCase) ||
                 gender.Equals("Female", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Gender must be either Male or Female.");
        RuleFor(request => request.ContactNumber)
            .NotEmpty()
            .Matches(@"^\+?[0-9]{10,15}$")
            .MaximumLength(30);
    }
}
