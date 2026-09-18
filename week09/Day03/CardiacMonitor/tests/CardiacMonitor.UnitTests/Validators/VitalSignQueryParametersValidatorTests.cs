using CardiacMonitor.DTOs;
using CardiacMonitor.Validators;

namespace CardiacMonitor.UnitTests.Validators;

public class VitalSignQueryParametersValidatorTests
{
    private readonly VitalSignQueryParametersValidator _validator = new();

    // Verifies that unsafe page sizes are rejected.
    [Fact]
    public async Task ValidateAsync_ReturnsError_WhenPageSizeExceedsMaximum()
    {
        // Arrange
        var query = new VitalSignQueryParameters(PageSize: 101);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(VitalSignQueryParameters.PageSize));
    }

    // Verifies that an inverted date range is rejected.
    [Fact]
    public async Task ValidateAsync_ReturnsError_WhenDateRangeIsInverted()
    {
        // Arrange
        var query = new VitalSignQueryParameters(
            From: new DateTime(2026, 8, 2),
            To: new DateTime(2026, 8, 1));

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(VitalSignQueryParameters.From));
    }

    // Verifies that a supported query combination passes validation.
    [Fact]
    public async Task ValidateAsync_ReturnsValid_WhenQueryIsSupported()
    {
        // Arrange
        var query = new VitalSignQueryParameters(
            Page: 2,
            PageSize: 10,
            MinHeartRate: 60,
            MaxHeartRate: 120,
            Sort: "heartRate_desc");

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        Assert.True(result.IsValid);
    }
}
