
using CardiacMonitor.Helpers; // استيراد كود المشروع الرئيسي لكي نستطيع فحصه
using Xunit;

namespace CardiacMonitor.Tests;


public class CardiacCalculatorTests
{
    private readonly CardiacCalculator _calculator;

    public CardiacCalculatorTests()
    {
        _calculator = new CardiacCalculator();
    }

    [Fact]
    public void IsBloodPressureNormal_WhenValuesAreIdeal_ReturnsTrue()
    {
        int systolic = 115;
        int diastolic = 75;

        bool result = _calculator.IsBloodPressureNormal(systolic, diastolic);

        Assert.True(result);
    }

    [Fact]
    public void CalculateMaxHeartRate_WhenAgeIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        int invalidAge = -5;

        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.CalculateMaxHeartRate(invalidAge));
    }

    [Theory]
    [InlineData(20, 193)]
    [InlineData(50, 172)]
    [InlineData(80, 151)]
    public void CalculateMaxHeartRate_WhenAgeIsValid_ReturnsExpectedHeartRate(int age, int expectedMaxHeartRate)
    {
        int result = _calculator.CalculateMaxHeartRate(age);

        Assert.Equal(expectedMaxHeartRate, result);
    }
}