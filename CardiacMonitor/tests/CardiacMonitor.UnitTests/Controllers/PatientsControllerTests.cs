using System.Security.Claims;
using CardiacMonitor.Controllers;
using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CardiacMonitor.UnitTests.Controllers;

public class PatientsControllerTests
{
    // Verifies that a missing patient produces a standardized not-found response.
    [Fact]
    public async Task GetById_ReturnsProblemDetails_WhenPatientDoesNotExist()
    {
        // Arrange
        var patientService = new Mock<IPatientService>();
        patientService
            .Setup(service => service.GetPatientByIdAsync(999))
            .ReturnsAsync((PatientResponse?)null);

        var controller = CreateController(patientService.Object, "admin-user", "Admin");

        // Act
        var result = await controller.GetById(999);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        Assert.Equal("Patient not found.", problemDetails.Title);
        patientService.Verify(
            service => service.GetPatientByIdAsync(999),
            Times.Once);
    }

    // Verifies that a patient cannot read another patient's profile.
    [Fact]
    public async Task GetById_ReturnsForbidden_WhenPatientDoesNotOwnProfile()
    {
        // Arrange
        var patientService = new Mock<IPatientService>();
        var patient = new PatientResponse(
            1,
            "owner-user",
            "Demo",
            "Patient",
            new DateTime(1990, 1, 1),
            "Male",
            "+970599123456");

        patientService
            .Setup(service => service.GetPatientByIdAsync(1))
            .ReturnsAsync(patient);

        var controller = CreateController(
            patientService.Object,
            "different-user",
            "Patient");

        // Act
        var result = await controller.GetById(1);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        Assert.Equal("Access forbidden.", problemDetails.Title);
    }

    // Verifies that a patient can read the profile linked to their identity user.
    [Fact]
    public async Task GetById_ReturnsOk_WhenPatientOwnsProfile()
    {
        // Arrange
        var patientService = new Mock<IPatientService>();
        var patient = new PatientResponse(
            1,
            "owner-user",
            "Demo",
            "Patient",
            new DateTime(1990, 1, 1),
            "Male",
            "+970599123456");

        patientService
            .Setup(service => service.GetPatientByIdAsync(1))
            .ReturnsAsync(patient);

        var controller = CreateController(
            patientService.Object,
            "owner-user",
            "Patient");

        // Act
        var result = await controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(patient, okResult.Value);
    }

    // Creates a controller with an authenticated user for authorization tests.
    private static PatientsController CreateController(
        IPatientService patientService,
        string userId,
        string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };

        var controller = new PatientsController(patientService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                }
            }
        };

        controller.HttpContext.Request.Path = "/api/patients/1";
        return controller;
    }
}
