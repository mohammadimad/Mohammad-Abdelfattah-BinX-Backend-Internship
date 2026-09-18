using CardiacMonitor.DTOs;
using CardiacMonitor.Validators;

namespace CardiacMonitor.UnitTests.Validators;

public class PatientQueryParametersValidatorTests
{
    private readonly PatientQueryParametersValidator _validator = new();

    // Verifies that unsafe page sizes are rejected.
    [Fact]
    public async Task ValidateAsync_ReturnsError_WhenPageSizeExceedsMaximum()
    {
        // Arrange
        var query = new PatientQueryParameters(PageSize: 101);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(PatientQueryParameters.PageSize));
    }

    // Verifies that unsupported gender and sort values are rejected.
    [Fact]
    public async Task ValidateAsync_ReturnsErrors_WhenGenderAndSortAreUnsupported()
    {
        // Arrange
        var query = new PatientQueryParameters(
            Gender: "Unknown",
            Sort: "id_random");

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(PatientQueryParameters.Gender));
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(PatientQueryParameters.Sort));
    }

    // Verifies that a supported patient query passes validation.
    [Fact]
    public async Task ValidateAsync_ReturnsValid_WhenQueryIsSupported()
    {
        // Arrange
        var query = new PatientQueryParameters(
            Page: 2,
            PageSize: 10,
            Search: "Sara Ali",
            Gender: "female",
            Sort: "dateOfBirth_desc");

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        Assert.True(result.IsValid);
    }
}
