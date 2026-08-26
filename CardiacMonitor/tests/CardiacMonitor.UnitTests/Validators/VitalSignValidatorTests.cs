using CardiacMonitor.DTOs;
using CardiacMonitor.Validators;

namespace CardiacMonitor.UnitTests.Validators;

public class VitalSignValidatorTests
{
    // Verifies that valid physiological input passes validation.
    [Fact]
    public void Validate_ReturnsValidResult_WhenVitalSignsAreWithinRange()
    {
        // Arrange
        var validator = new CreateVitalSignRequestValidator();
        var request = new CreateVitalSignRequest(75, 98.5m, 120, 80);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    // Verifies that heart rates outside the allowed range are rejected.
    [Theory]
    [InlineData(29)]
    [InlineData(251)]
    [InlineData(500)]
    public void Validate_ReturnsInvalidResult_WhenHeartRateIsOutsideRange(int heartRate)
    {
        // Arrange
        var validator = new CreateVitalSignRequestValidator();
        var request = new CreateVitalSignRequest(heartRate, 98m, 120, 80);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateVitalSignRequest.HeartRate));
    }

    // Verifies that oxygen saturation outside the allowed range is rejected.
    [Theory]
    [InlineData("49.9")]
    [InlineData("100.1")]
    public void Validate_ReturnsInvalidResult_WhenOxygenSaturationIsOutsideRange(
        string oxygenSaturation)
    {
        // Arrange
        var validator = new CreateVitalSignRequestValidator();
        var request = new CreateVitalSignRequest(
            75,
            decimal.Parse(oxygenSaturation),
            120,
            80);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateVitalSignRequest.OxygenSaturation));
    }
}
