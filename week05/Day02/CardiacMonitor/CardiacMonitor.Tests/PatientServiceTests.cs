using CardiacMonitor.Models;
using CardiacMonitor.Repositories;
using CardiacMonitor.Services;
using Moq;
using Xunit;

namespace CardiacMonitor.Tests.Services;

public class PatientServiceTests
{
    [Fact]
    public async Task GetPatientByIdAsync_WhenPatientExists_ReturnsMappedPatient()
    {
        // Arrange
        var patientId = 1;
        var patient = CreatePatient(patientId);
        var mockRepository = new Mock<IPatientRepository>();

        mockRepository
            .Setup(repository => repository.GetByIdAsync(patientId, false))
            .ReturnsAsync(patient);
        var service = new PatientService(mockRepository.Object);

        // Act
        var result = await service.GetPatientByIdAsync(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(patient.Id, result.Id);
        Assert.Equal(patient.FirstName, result.FirstName);
        Assert.Equal(patient.LastName, result.LastName);

        mockRepository.Verify(
            repository => repository.GetByIdAsync(patientId, false),
            Times.Once);
    }

    [Fact]
    public async Task GetPatientByIdAsync_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        var patientId = 1;
        var mockRepository = new Mock<IPatientRepository>();

        mockRepository
            .Setup(repository => repository.GetByIdAsync(patientId, false))
            .Throws<InvalidOperationException>();
        var service = new PatientService(mockRepository.Object);

        // Act
        var action = () => service.GetPatientByIdAsync(patientId);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            action);

        mockRepository.Verify(
            repository => repository.GetByIdAsync(patientId, false),
            Times.Once);
    }

    private static Patient CreatePatient(int id)
    {
        return new Patient
        {
            Id = id,
            FirstName = "Ahmad",
            LastName = "Amr",
            DateOfBirth = new DateTime(1990, 5, 12),
            Gender = "Male",
            ContactNumber = "+9759835279"
        };
    }
}
