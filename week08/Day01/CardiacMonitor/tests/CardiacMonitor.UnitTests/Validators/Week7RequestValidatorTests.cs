using CardiacMonitor.DTOs;
using CardiacMonitor.Validators;

namespace CardiacMonitor.UnitTests.Validators;

public class Week7RequestValidatorTests
{
    // Verifies that a care assignment requires a valid Nurse profile ID.
    [Fact]
    public async Task CareAssignmentValidator_ReturnsError_WhenNurseIdIsInvalid()
    {
        // Arrange
        var validator = new CreateCareAssignmentRequestValidator();
        var request = new CreateCareAssignmentRequest(0, "Demo");

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(CreateCareAssignmentRequest.NurseProfileId));
    }
}
