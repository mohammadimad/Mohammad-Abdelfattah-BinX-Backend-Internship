# Week 5 - Day 1: Unit Testing with xUnit

## Day Overview

Day 1 focused on selecting the Phase 3 capstone project and learning the foundations of unit testing with xUnit. The selected project is the **Cardiac Patient Monitoring API**, a healthcare backend for patients, vital signs, medications, appointments, authentication, and role-based access.

## What We Learned

- The difference between `[Fact]` and `[Theory]` tests.
- How `[InlineData]` runs one theory with several input cases.
- How to organize a test with the Arrange-Act-Assert pattern.
- Why a unit test should examine one small piece of logic without external dependencies.
- How clear test names describe the method, scenario, and expected result.

## Tasks We Completed

### Task 1: Create an xUnit test project

The test project references the main API project, allowing tests to call production classes directly.

```xml
<ProjectReference Include="..\CardiacMonitor.csproj" />
```

### Task 2: Write three `[Fact]` tests

The facts cover normal blood pressure, high blood pressure, and invalid age input.

```csharp
[Fact]
public void IsBloodPressureNormal_WhenValuesAreIdeal_ReturnsTrue()
{
    int systolic = 115;
    int diastolic = 75;

    bool result = _calculator.IsBloodPressureNormal(systolic, diastolic);

    Assert.True(result);
}
```

### Task 3: Write one `[Theory]` with at least three cases

The same maximum-heart-rate test runs once for every `InlineData` row.

```csharp
[Theory]
[InlineData(20, 193)]
[InlineData(50, 172)]
[InlineData(80, 151)]
public void CalculateMaxHeartRate_WhenAgeIsValid_ReturnsExpectedHeartRate(
    int age,
    int expectedMaxHeartRate)
{
    int result = _calculator.CalculateMaxHeartRate(age);

    Assert.Equal(expectedMaxHeartRate, result);
}
```

## Files Related to Day 1

- `Helpers/CardiacCalculator.cs`
- `CardiacMonitor.Tests/CardiacCalculatorTests.cs`
- `CardiacMonitor.Tests/CardiacMonitor.Tests.csproj`

## Day Result

The project has readable xUnit tests that demonstrate `[Fact]`, `[Theory]`, `InlineData`, exception assertions, and Arrange-Act-Assert.
