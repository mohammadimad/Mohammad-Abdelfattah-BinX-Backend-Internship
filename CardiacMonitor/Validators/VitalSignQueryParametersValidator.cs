using CardiacMonitor.DTOs;
using FluentValidation;

namespace CardiacMonitor.Validators;

public sealed class VitalSignQueryParametersValidator
    : AbstractValidator<VitalSignQueryParameters>
{
    private static readonly string[] SupportedSortValues =
    {
        "recordedAt_desc",
        "recordedAt_asc",
        "heartRate_desc",
        "heartRate_asc"
    };

    // Defines safe pagination, filter, and sorting rules for vital-sign queries.
    public VitalSignQueryParametersValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than zero.");
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
        RuleFor(query => query.From)
            .LessThanOrEqualTo(query => query.To)
            .When(query => query.From.HasValue && query.To.HasValue)
            .WithMessage("From must be earlier than or equal to To.");
        RuleFor(query => query.MinHeartRate)
            .InclusiveBetween(30, 250)
            .When(query => query.MinHeartRate.HasValue);
        RuleFor(query => query.MaxHeartRate)
            .InclusiveBetween(30, 250)
            .When(query => query.MaxHeartRate.HasValue);
        RuleFor(query => query.MinHeartRate)
            .LessThanOrEqualTo(query => query.MaxHeartRate)
            .When(query => query.MinHeartRate.HasValue && query.MaxHeartRate.HasValue)
            .WithMessage("Minimum heart rate must not exceed maximum heart rate.");
        RuleFor(query => query.Sort)
            .Must(sort => SupportedSortValues.Contains(sort, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Sort must be one of: {string.Join(", ", SupportedSortValues)}.");
    }
}
