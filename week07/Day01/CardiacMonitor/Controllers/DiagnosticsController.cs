using Microsoft.AspNetCore.Mvc;

namespace CardiacMonitor.Controllers;

[ApiController]
[Route("api/diagnostics")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class DiagnosticsController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

     ///The environment name is saved so that the diagnostic endpoint remains disabled in Production.
     public DiagnosticsController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    
    // Throws a deliberate exception outside of Production so we can check the general middleware.
    [HttpGet("unhandled-error")]
    public IActionResult ThrowUnhandledError()
    {
        var canRunDiagnostic =
            _environment.IsDevelopment() || _environment.IsEnvironment("Testing");

        if (!canRunDiagnostic)
        {
            return NotFound();
        }

        throw new InvalidOperationException(
            "Diagnostic exception details must never be returned to the client.");
    }
}
