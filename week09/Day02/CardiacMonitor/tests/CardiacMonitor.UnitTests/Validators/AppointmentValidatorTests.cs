using CardiacMonitor.DTOs;
using CardiacMonitor.Validators;

namespace CardiacMonitor.UnitTests.Validators;

public class AppointmentValidatorTests
{
    // Verifies that appointments in the past are rejected.
    [Fact]
    public void Validate_ReturnsInvalidResult_WhenAppointmentDateIsInThePast()
    {
        // Arrange
        var validator = new CreateAppointmentRequestValidator();
        var request = new CreateAppointmentRequest(
            "doctor-id",
            DateTime.UtcNow.AddMinutes(-5),
            "Scheduled",
            null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateAppointmentRequest.AppointmentDate));
    }

    // Verifies that unsupported appointment statuses are rejected.
    [Theory]
    [InlineData("Pending")]
    [InlineData("Unknown")]
    [InlineData("")]
    public void Validate_ReturnsInvalidResult_WhenStatusIsUnsupported(string status)
    {
        // Arrange
        var validator = new CreateAppointmentRequestValidator();
        var request = new CreateAppointmentRequest(
            "doctor-id",
            DateTime.UtcNow.AddDays(1),
            status,
            null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateAppointmentRequest.Status));
    }
}
