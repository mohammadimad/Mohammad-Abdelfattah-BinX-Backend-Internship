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
    //Moq
    private readonly Mock<IPatientService> _mockPatientService;
    private readonly PatientsController _controller;

    public PatientsControllerTests()
    {
        _mockPatientService = new Mock<IPatientService>();

        // Object 
        _controller = new PatientsController(_mockPatientService.Object);
    }

    [Fact]
    public async Task GetById_WhenPatientExists_ReturnsOkResultWithPatient()
    {
        // Arrange
        var patientId = 1;
        var expectedPatient = CreatePatientResponse(patientId);

        // Setup وReturnsAsync  
        _mockPatientService
            .Setup(service => service.GetPatientByIdAsync(patientId))
            .ReturnsAsync(expectedPatient);
        SetDoctorUser();

        // Act
        var result = await _controller.GetById(patientId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expectedPatient, okResult.Value);

        // Verify 
        _mockPatientService.Verify(
            service => service.GetPatientByIdAsync(patientId),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenServiceThrows_PropagatesException()
    {
        // Arrange
        var patientId = 1;

        // ThrowsAsync  
        _mockPatientService
            .Setup(service => service.GetPatientByIdAsync(patientId))
            .ThrowsAsync(new InvalidOperationException("Database failure"));

        // Act + Assert 
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.GetById(patientId));

        _mockPatientService.Verify(
            service => service.GetPatientByIdAsync(patientId),
            Times.Once);
    }

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
