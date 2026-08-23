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
    // Moq It creates a fake service alternative so we can test the controller without a real database..
     private readonly Mock<IPatientService> _mockPatientService;
    private readonly PatientsController _controller;

    public PatientsControllerTests()
    {
        _mockPatientService = new Mock<IPatientService>();

        _controller = new PatientsController(_mockPatientService.Object);
    }

    [Fact]
    public async Task GetById_WhenPatientExists_ReturnsOkResultWithPatient()
    {
        // Arrange
        var patientId = 1;
        var expectedPatient = CreatePatientResponse(patientId, null);

        // Setup وReturnsAsync  
        _mockPatientService
            .Setup(service => service.GetPatientByIdAsync(patientId))
            .ReturnsAsync(expectedPatient);
        SetAuthenticatedUser("doctor-id-123", "Doctor");

        // Act
        var result = await _controller.GetById(patientId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expectedPatient, okResult.Value);

        // Verify Ensure that the requested function was called only once and with the same patient number..
        _mockPatientService.Verify(
            service => service.GetPatientByIdAsync(patientId),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenServiceThrows_PropagatesException()
    {
        // Arrange
        var patientId = 1;

        // ThrowsAsync It simulates external dependency failure without the need to corrupt a real database..
        _mockPatientService
            .Setup(service => service.GetPatientByIdAsync(patientId))
            .ThrowsAsync(new InvalidOperationException("Database failure"));

        // Act + Assert:We leave the exception to the general middleware instead of repeating try/catch at every endpoint.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.GetById(patientId));

        _mockPatientService.Verify(
            service => service.GetPatientByIdAsync(patientId),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenPatientRequestsAnotherProfile_ReturnsForbidden()
    {
        // Arrange: This is a security risk; the current patient does not have the required record..
        var patientId = 1;
        var patient = CreatePatientResponse(patientId, "owner-user-id");

        _mockPatientService
            .Setup(service => service.GetPatientByIdAsync(patientId))
            .ReturnsAsync(patient);
        SetAuthenticatedUser("different-user-id", "Patient");

        // Act
        var result = await _controller.GetById(patientId);

        // Assert
        Assert.IsType<ForbidResult>(result);
        _mockPatientService.Verify(
            service => service.GetPatientByIdAsync(patientId),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenPatientRequestsOwnProfile_ReturnsOk()
    {
        // Arrange:The user number in JWT matches the patient record owner.
        var patientId = 1;
        var ownerUserId = "owner-user-id";
        var patient = CreatePatientResponse(patientId, ownerUserId);

        _mockPatientService
            .Setup(service => service.GetPatientByIdAsync(patientId))
            .ReturnsAsync(patient);
        SetAuthenticatedUser(ownerUserId, "Patient");

        // Act
        var result = await _controller.GetById(patientId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(patient, okResult.Value);
        _mockPatientService.Verify(
            service => service.GetPatientByIdAsync(patientId),
            Times.Once);
    }

    private void SetAuthenticatedUser(string userId, string role)
    {
           
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    private static PatientResponse CreatePatientResponse(int id, string? userId)
    {
        return new PatientResponse(
            id,
            userId,
            "Ahmad",
            "Amr",
            new DateTime(1990, 5, 12),
            "Male",
            "+9759835279");
    }
}
