using CardiacMonitor.DTOs;
using FluentValidation;

namespace CardiacMonitor.Validators;

public sealed class CreateDoctorAvailabilityRequestValidator
    : AbstractValidator<CreateDoctorAvailabilityRequest>
{
    // Defines valid weekly day and time ranges for Doctor availability.
    public CreateDoctorAvailabilityRequestValidator()
    {
        RuleFor(request => request.DayOfWeek).IsInEnum();
        RuleFor(request => request.StartTime)
            .LessThan(request => request.EndTime)
            .WithMessage("Start time must be earlier than end time.");
    }
}
