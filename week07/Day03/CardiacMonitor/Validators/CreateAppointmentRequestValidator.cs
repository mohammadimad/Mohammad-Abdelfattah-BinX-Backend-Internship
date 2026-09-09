using CardiacMonitor.DTOs;
using FluentValidation;

namespace CardiacMonitor.Validators;

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("Doctor ID is required.");

        RuleFor(x => x.AppointmentDate)
            .NotEmpty().WithMessage("Appointment date is required.")
            .GreaterThan(DateTime.UtcNow).WithMessage("Appointment date must be in the future.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(x => x == "Scheduled" || x == "Completed" || x == "Cancelled")
            .WithMessage("Status must be: Scheduled, Completed, or Cancelled.");
    }
}