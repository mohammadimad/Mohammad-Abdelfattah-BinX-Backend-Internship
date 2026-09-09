using CardiacMonitor.DTOs;
using FluentValidation;

namespace CardiacMonitor.Validators;

public class CreateMedicationOrderRequestValidator : AbstractValidator<CreateMedicationOrderRequest>
{
    // Configures validation rules for medication order requests.
    public CreateMedicationOrderRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one medication item is required.")
            .Must(items => items is null || items.Select(item => item.MedicationId).Distinct().Count() == items.Count)
            .WithMessage("Each medication can appear only once in an order.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.MedicationId)
                .GreaterThan(0).WithMessage("Medication ID must be greater than zero.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
