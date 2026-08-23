using CardiacMonitor.DTOs;
using FluentValidation;

namespace CardiacMonitor.Validators;

public class CreateVitalSignRequestValidator : AbstractValidator<CreateVitalSignRequest>
{
    public CreateVitalSignRequestValidator()
    {
        RuleFor(x => x.HeartRate)
            .InclusiveBetween(30, 250).WithMessage("Heart rate must be between 30 and 250 bpm.");

        RuleFor(x => x.OxygenSaturation)
            .InclusiveBetween(50.0m, 100.0m).WithMessage("Oxygen saturation must be between 50% and 100%.");

        RuleFor(x => x.SystolicBP)
            .InclusiveBetween(70, 220).WithMessage("Systolic BP must be between 70 and 220 mmHg.");

        RuleFor(x => x.DiastolicBP)
            .InclusiveBetween(40, 130).WithMessage("Diastolic BP must be between 40 and 130 mmHg.");
    }
}