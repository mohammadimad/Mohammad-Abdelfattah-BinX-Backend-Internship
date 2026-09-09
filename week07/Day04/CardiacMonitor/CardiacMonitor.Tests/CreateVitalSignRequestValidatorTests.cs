using CardiacMonitor.DTOs;
using CardiacMonitor.Validators;
using Xunit;

namespace CardiacMonitor.Tests.Validators;

public class CreateVitalSignRequestValidatorTests
{
    // This is one of the highest risks in the project because accepting an incorrect medical reading could corrupt patient data..
    private readonly CreateVitalSignRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenClinicalReadingsAreWithinBoundaries_IsValid()
    {
        // Arrange: We use the same limits allowed in the project rules..
        var request = new CreateVitalSignRequest(
            HeartRate: 30,
            OxygenSaturation: 100m,
            SystolicBP: 220,
            DiastolicBP: 40);

        // Act
        var result = _validator.Validate(request);

        // Assert:The values ​​falling on the same boundary must be correct..
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenClinicalReadingsAreOutsideSafeRanges_ReturnsEveryError()
    {
        // Arrange: Every value here falls outside the allowed range..
        var request = new CreateVitalSignRequest(
            HeartRate: 29,
            OxygenSaturation: 49.9m,
            SystolicBP: 221,
            DiastolicBP: 39);

        // Act
        var result = _validator.Validate(request);

        // Assert: We ensure that the checker returned a separate error for each incorrect reading..
        Assert.False(result.IsValid);
        Assert.Collection(
            result.Errors.OrderBy(error => error.PropertyName),
            error => Assert.Equal(nameof(request.DiastolicBP), error.PropertyName),
            error => Assert.Equal(nameof(request.HeartRate), error.PropertyName),
            error => Assert.Equal(nameof(request.OxygenSaturation), error.PropertyName),
            error => Assert.Equal(nameof(request.SystolicBP), error.PropertyName));
    }
}
