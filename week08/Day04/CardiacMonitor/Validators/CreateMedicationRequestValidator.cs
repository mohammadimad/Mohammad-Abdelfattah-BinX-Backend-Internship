using CardiacMonitor.DTOs;
using FluentValidation;

namespace CardiacMonitor.Validators;

public class CreateMedicationRequestValidator : AbstractValidator<CreateMedicationRequest>
{
    public CreateMedicationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Medication name is required.")
            .MaximumLength(100).WithMessage("Medication name cannot exceed 100 characters.");

        RuleFor(x => x.Dosage)
            .NotEmpty().WithMessage("Dosage is required.");

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("Frequency is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after start date.");
    }
}