
using CardiacMonitor.Helpers;
using Xunit;

namespace CardiacMonitor.Tests;


public class CardiacCalculatorTests
{
    private readonly CardiacCalculator _calculator;

    // Creates the pure calculator used by every unit test.
    public CardiacCalculatorTests()
    {
        _calculator = new CardiacCalculator();
    }

    // Verifies that ideal blood-pressure values are considered normal.
    [Fact]
    public void IsBloodPressureNormal_WhenValuesAreIdeal_ReturnsTrue()
    {
        // Arrange
        int systolic = 115;
        int diastolic = 75;

        // Act
        bool result = _calculator.IsBloodPressureNormal(systolic, diastolic);

        // Assert
        Assert.True(result);
    }

    // Verifies that a high systolic value is not considered normal.
    [Fact]
    public void IsBloodPressureNormal_WhenSystolicIsHigh_ReturnsFalse()
    {
        // Arrange
        int systolic = 130;
        int diastolic = 75;

        // Act
        bool result = _calculator.IsBloodPressureNormal(systolic, diastolic);

        // Assert
        Assert.False(result);
    }

    // Verifies that an invalid age throws the expected exception.
    [Fact]
    public void CalculateMaxHeartRate_WhenAgeIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int invalidAge = -5;

        // Act and Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.CalculateMaxHeartRate(invalidAge));
    }

    // Verifies the heart-rate formula with three InlineData cases.
    [Theory]
    [InlineData(20, 193)]
    [InlineData(50, 172)]
    [InlineData(80, 151)]
    public void CalculateMaxHeartRate_WhenAgeIsValid_ReturnsExpectedHeartRate(int age, int expectedMaxHeartRate)
    {
        // Act
        int result = _calculator.CalculateMaxHeartRate(age);

        // Assert
        Assert.Equal(expectedMaxHeartRate, result);
    }
}
