namespace CardiacMonitor.UnitTests;

// A lab-only assertion used to demonstrate failure propagation to the CI job.
public class CiPipelineVerificationTests
{
    [Fact]
    public void PipelineVerification_DetectsFailure()
    {
       Assert.Equal(2, 1 + 2); // Corrected after observing the intentional failure.
    }
}
