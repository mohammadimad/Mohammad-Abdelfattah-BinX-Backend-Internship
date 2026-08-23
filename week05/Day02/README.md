# Week 5 - Day 2: Mocking Dependencies with Moq

## Day Overview

Day 2 focused on isolating a service from its database dependency. A repository interface was introduced so `PatientService` could be tested with Moq instead of connecting to a real database.

## What We Learned

- Why unit tests replace slow or external dependencies with mocks.
- How `Mock<T>` creates a controlled implementation of an interface.
- How `Setup` selects the method call being configured.
- How `ReturnsAsync` supplies a controlled result.
- How `ThrowsAsync` simulates a dependency failure.
- How `Verify` and `Times.Once` confirm an expected interaction.

## Tasks We Completed

### Task 1: Depend on a repository interface

`PatientService` depends on `IPatientRepository`, while `PatientRepository` contains the real EF Core database code.

```csharp
public interface IPatientRepository
{
    Task<IReadOnlyList<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id, bool trackChanges = false);
    Task AddAsync(Patient patient);
    void Remove(Patient patient);
    Task SaveChangesAsync();
}
```

### Task 2: Mock a successful repository result

The repository returns a known patient, and the test verifies that the service maps it to a response correctly.

```csharp
_mockRepository
    .Setup(repository => repository.GetByIdAsync(patientId, false))
    .ReturnsAsync(patient);

var result = await _service.GetPatientByIdAsync(patientId);

Assert.NotNull(result);
Assert.Equal(patient.Id, result.Id);
```

### Task 3: Mock an exception

`ThrowsAsync` simulates a database failure without requiring a real database failure.

```csharp
_mockRepository
    .Setup(repository => repository.GetByIdAsync(patientId, false))
    .ThrowsAsync(new InvalidOperationException("Database failure"));

await Assert.ThrowsAsync<InvalidOperationException>(
    () => _service.GetPatientByIdAsync(patientId));
```

### Task 4: Verify the dependency interaction

The test confirms that the repository method was called exactly once with the correct patient ID.

```csharp
_mockRepository.Verify(
    repository => repository.GetByIdAsync(patientId, false),
    Times.Once);
```

## Files Related to Day 2

- `Repositories/IPatientRepository.cs`
- `Repositories/PatientRepository.cs`
- `Services/PatientService.cs`
- `CardiacMonitor.Tests/PatientServiceTests.cs`

## Day Result

The patient service can now be unit tested in isolation using only xUnit and Moq, following the dependency-mocking pattern from the material.
