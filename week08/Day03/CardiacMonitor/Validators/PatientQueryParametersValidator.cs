using CardiacMonitor.DTOs;
using FluentValidation;

namespace CardiacMonitor.Validators;

public sealed class PatientQueryParametersValidator
    : AbstractValidator<PatientQueryParameters>
{
    private static readonly string[] SupportedSortValues =
    {
        "firstName_asc",
        "firstName_desc",
        "lastName_asc",
        "lastName_desc",
        "dateOfBirth_asc",
        "dateOfBirth_desc"
    };

    // Defines safe pagination, search, gender, and sorting rules for patient queries.
    public PatientQueryParametersValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than zero.");
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
        RuleFor(query => query.Search)
            .MaximumLength(100)
            .WithMessage("Search must not exceed 100 characters.");
        RuleFor(query => query.Gender)
            .Must(gender =>
                string.IsNullOrWhiteSpace(gender) ||
                gender.Equals("Male", StringComparison.OrdinalIgnoreCase) ||
                gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Gender must be Male or Female.");
        RuleFor(query => query.Sort)
            .Must(sort => SupportedSortValues.Contains(
                sort,
                StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Sort must be one of: {string.Join(", ", SupportedSortValues)}.");
    }
}
