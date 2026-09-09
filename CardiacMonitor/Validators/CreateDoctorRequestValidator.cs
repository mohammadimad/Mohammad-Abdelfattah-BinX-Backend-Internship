using CardiacMonitor.DTOs;
using FluentValidation;

namespace CardiacMonitor.Validators;

public sealed class CreateDoctorRequestValidator
    : AbstractValidator<CreateDoctorRequest>
{
    // Defines identity and professional-profile rules for Doctor creation.
    public CreateDoctorRequestValidator()
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
        RuleFor(request => request.FullName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);
        RuleFor(request => request.LicenseNumber)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50);
        RuleFor(request => request.Specialty)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
        RuleFor(request => request.Department)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
    }
}
