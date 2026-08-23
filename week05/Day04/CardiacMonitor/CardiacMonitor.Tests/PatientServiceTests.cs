using CardiacMonitor.Models;
using CardiacMonitor.Repositories;
using CardiacMonitor.Services;
using Moq;
using Xunit;

namespace CardiacMonitor.Tests.Services;

public class PatientServiceTests
{
    private readonly Mock<IPatientRepository> _mockRepository;
    private readonly PatientService _service;

    // Creates a service with a mocked repository dependency.
    public PatientServiceTests()
    {
        _mockRepository = new Mock<IPatientRepository>();
        _service = new PatientService(_mockRepository.Object);
    }

    // Verifies that the service maps a repository entity to its response DTO.
    [Fact]
    public async Task GetPatientByIdAsync_WhenPatientExists_ReturnsMappedPatient()
    {
        // Arrange
        var patientId = 1;
        var patient = CreatePatient(patientId);
        _mockRepository
            .Setup(repository => repository.GetByIdAsync(patientId, false))
            .ReturnsAsync(patient);

        // Act
        var result = await _service.GetPatientByIdAsync(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(patient.Id, result.Id);
        Assert.Equal(patient.FirstName, result.FirstName);
        Assert.Equal(patient.LastName, result.LastName);
        _mockRepository.Verify(
            repository => repository.GetByIdAsync(patientId, false),
            Times.Once);
    }

    // Verifies that an unexpected repository error is allowed to bubble up.
    [Fact]
    public async Task GetPatientByIdAsync_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange: Simulate a repository failure with ThrowsAsync.
        var patientId = 1;
        _mockRepository
            .Setup(repository => repository.GetByIdAsync(patientId, false))
            .ThrowsAsync(new InvalidOperationException("Database failure"));

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetPatientByIdAsync(patientId));
        _mockRepository.Verify(
            repository => repository.GetByIdAsync(patientId, false),
            Times.Once);
    }

    // Creates the patient entity used by the service tests.
    private static Patient CreatePatient(int id) => new()
    {
        Id = id,
        FirstName = "Ahmad",
        LastName = "Amr",
        DateOfBirth = new DateTime(1990, 5, 12),
        Gender = "Male",
        ContactNumber = "+9759835279"
    };
}
