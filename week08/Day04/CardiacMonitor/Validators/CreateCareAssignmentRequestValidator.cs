using CardiacMonitor.DTOs;
using FluentValidation;

namespace CardiacMonitor.Validators;

public sealed class CreateCareAssignmentRequestValidator
    : AbstractValidator<CreateCareAssignmentRequest>
{
    // Defines valid nurse and notes values for a patient care assignment.
    public CreateCareAssignmentRequestValidator()
    {
        RuleFor(request => request.NurseProfileId).GreaterThan(0);
        RuleFor(request => request.Notes).MaximumLength(500);
    }
}
