using System.Security.Claims;
using CardiacMonitor.Controllers;
using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CardiacMonitor.Tests.Controllers;

public class PatientsControllerTests
{
    private readonly Mock<IPatientService> _mockPatientService;
    private readonly PatientsController _controller;

    // Creates a controller with a mocked patient service.
    public PatientsControllerTests()
    {
        _mockPatientService = new Mock<IPatientService>();

        _controller = new PatientsController(_mockPatientService.Object);
    }

    // Verifies that an existing patient produces an HTTP 200 result.
    [Fact]
    public async Task GetById_WhenPatientExists_ReturnsOkResultWithPatient()
    {
        // Arrange
        var patientId = 1;
        var expectedPatient = CreatePatientResponse(patientId);

        // Configure the value returned by the mocked service.
        _mockPatientService
            .Setup(service => service.GetPatientByIdAsync(patientId))
            .ReturnsAsync(expectedPatient);
        SetDoctorUser();

        // Act
        var result = await _controller.GetById(patientId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expectedPatient, okResult.Value);

        // Verify that the mocked method was called exactly once.
        _mockPatientService.Verify(
            service => service.GetPatientByIdAsync(patientId),
            Times.Once);
    }

    // Verifies that the controller does not hide an unexpected service error.
    [Fact]
    public async Task GetById_WhenServiceThrows_PropagatesException()
    {
        // Arrange
        var patientId = 1;

        // Simulate a dependency failure with ThrowsAsync.
        _mockPatientService
            .Setup(service => service.GetPatientByIdAsync(patientId))
            .ThrowsAsync(new InvalidOperationException("Database failure"));

        // Act and Assert: Let the exception bubble to the global middleware.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.GetById(patientId));

        _mockPatientService.Verify(
            service => service.GetPatientByIdAsync(patientId),
            Times.Once);
    }

    // Adds a doctor identity to the controller context.
    private void SetDoctorUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "doctor-id-123"),
            new Claim(ClaimTypes.Role, "Doctor")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    // Creates the patient response used by the controller test.
    private static PatientResponse CreatePatientResponse(int id)
    {
        return new PatientResponse(
            id,
            null,
            "Ahmad",
            "Amr",
            new DateTime(1990, 5, 12),
            "Male",
            "+9759835279");
    }
}
