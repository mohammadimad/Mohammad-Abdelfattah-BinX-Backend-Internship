using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CardiacMonitor.Infrastructure;

/// <summary>Adds illustrative JSON request and response examples to key clinical and authentication operations.</summary>
public sealed class ApiExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controller = context.ApiDescription.ActionDescriptor.RouteValues["controller"];
        var action = context.ApiDescription.ActionDescriptor.RouteValues["action"];
        var example = (controller, action) switch
        {
            ("Auth", "Register") => (200,
                """{"email":"patient@example.com","password":"ExamplePassword1!","firstName":"Lina","lastName":"Nasser","dateOfBirth":"1995-06-15","gender":"Female","contactNumber":"+970599111111"}""",
                """{"message":"Patient account and profile registered successfully."}"""),
            ("Auth", "Login") => (200,
                """{"email":"patient@example.com","password":"ExamplePassword1!"}""",
                """{"token":"ACCESS_TOKEN_PLACEHOLDER","refreshToken":"REFRESH_TOKEN_PLACEHOLDER","message":"Tokens generated successfully."}"""),
            ("VitalSigns", "CreateVital") => (201,
                """{"heartRate":75,"oxygenSaturation":98.5,"systolicBP":120,"diastolicBP":80}""",
                """{"id":10,"patientId":1,"heartRate":75,"oxygenSaturation":98.5,"systolicBP":120,"diastolicBP":80,"recordedAt":"2026-09-17T10:00:00Z"}"""),
            _ => (0, "", "")
        };
        if (example.Item1 == 0) return;

        if (operation.RequestBody != null)
        {
            foreach (var media in operation.RequestBody.Content.Values)
                media.Example = OpenApiAnyFactory.CreateFromJson(example.Item2);
        }
        var status = example.Item1.ToString();
        if (!operation.Responses.TryGetValue(status, out var response))
        {
            response = new OpenApiResponse { Description = "Successful operation." };
            operation.Responses[status] = response;
        }
        // IActionResult does not expose response DTOs automatically, so supply explicit schemas.
        var responseType = controller == "VitalSigns"
            ? typeof(DTOs.VitalSignResponse) : null;
        response.Content["application/json"] = new OpenApiMediaType
        {
            Schema = responseType == null
                ? new OpenApiSchema
                {
                    Type = "object",
                    Properties = controller == "Auth" && action == "Login"
                        ? new Dictionary<string, OpenApiSchema>
                        {
                            ["token"] = new() { Type = "string" },
                            ["refreshToken"] = new() { Type = "string" },
                            ["message"] = new() { Type = "string" }
                        }
                        : new Dictionary<string, OpenApiSchema>
                        {
                            ["message"] = new() { Type = "string" }
                        }
                }
                : context.SchemaGenerator.GenerateSchema(responseType, context.SchemaRepository),
            Example = OpenApiAnyFactory.CreateFromJson(example.Item3)
        };
    }
}
