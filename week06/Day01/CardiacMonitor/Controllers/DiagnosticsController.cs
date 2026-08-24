using Microsoft.AspNetCore.Mvc;

namespace CardiacMonitor.Controllers;

[ApiController]
[Route("api/diagnostics")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class DiagnosticsController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// يحفظ اسم البيئة حتى يبقى endpoint التشخيص معطلاً في Production.
    /// </summary>
    public DiagnosticsController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// يرمي استثناءً متعمدًا خارج Production حتى نتحقق من الـ middleware العام.
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
