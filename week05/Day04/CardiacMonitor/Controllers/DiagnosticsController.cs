using Microsoft.AspNetCore.Mvc;

namespace CardiacMonitor.Controllers;

[ApiController]
[Route("api/diagnostics")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class DiagnosticsController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Stores the environment so the diagnostic endpoint stays disabled in production.
    /// </summary>
    public DiagnosticsController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Throws a deliberate exception to verify the global middleware outside production.
    /// </summary>
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
