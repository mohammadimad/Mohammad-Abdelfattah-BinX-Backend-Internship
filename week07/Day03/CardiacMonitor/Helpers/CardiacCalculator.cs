using System;
namespace CardiacMonitor.Helpers
{

    public class CardiacCalculator
    {
        //Given a patient's age, calculate the maximum heart rate using the formula: 207 - (0.7 * age)
        public int CalculateMaxHeartRate(int age)
        {
            if (age <= 0 || age > 120)
                throw new ArgumentOutOfRangeException(nameof(age), "Age must be between 1 and 120.");

            return (int)Math.Round(207 - (0.7 * age));
        }

        //Function to check if blood pressure is normal or high
        public bool IsBloodPressureNormal(int systolic, int diastolic)
        {
            return systolic < 120 && diastolic < 80;
        }
    }
}
