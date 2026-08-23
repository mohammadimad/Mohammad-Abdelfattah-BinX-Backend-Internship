
using CardiacMonitor.Helpers;  
using Xunit;

namespace CardiacMonitor.Tests;


public class CardiacCalculatorTests
{
    [Fact]
    public void IsBloodPressureNormal_WhenValuesAreIdeal_ReturnsTrue()
    {
        // Arrang  
        var calculator = new CardiacCalculator();
        int systolic = 115;
        int diastolic = 75;

        // Act 
        bool result = calculator.IsBloodPressureNormal(systolic, diastolic);

        // Assert 
        Assert.True(result);
    }

    [Fact]
    public void IsBloodPressureNormal_WhenSystolicIsHigh_ReturnsFalse()
    {
        // Arrange
        var calculator = new CardiacCalculator();
        int systolic = 130;
        int diastolic = 75;

        // Act
        bool result = calculator.IsBloodPressureNormal(systolic, diastolic);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsBloodPressureNormal_WhenDiastolicIsHigh_ReturnsFalse()
    {
        // Arrange
        var calculator = new CardiacCalculator();
        int systolic = 115;
        int diastolic = 85;

        // Act
        bool result = calculator.IsBloodPressureNormal(systolic, diastolic);

        // Assert
        Assert.False(result);
    }

    // Theory    
    [Theory]
    [InlineData(20, 193)]
    [InlineData(50, 172)]
    [InlineData(80, 151)]
    public void CalculateMaxHeartRate_WhenAgeIsValid_ReturnsExpectedHeartRate(int age, int expectedMaxHeartRate)
    {
        // Arrange
        var calculator = new CardiacCalculator();

        // Act
        int result = calculator.CalculateMaxHeartRate(age);

        // Assert
        Assert.Equal(expectedMaxHeartRate, result);
    }
}
