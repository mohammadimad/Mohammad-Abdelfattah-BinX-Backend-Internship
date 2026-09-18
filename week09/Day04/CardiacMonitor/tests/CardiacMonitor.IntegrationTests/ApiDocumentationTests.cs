using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace CardiacMonitor.IntegrationTests;

public class ApiDocumentationTests
{
    [Theory]
    [InlineData("/api/auth/register", "Post")]
    [InlineData("/api/auth/login", "Post")]
    [InlineData("/api/auth/refresh", "Post")]
    [InlineData("/api/patients", "Get")]
    [InlineData("/api/patients/{id}", "Get")]
    [InlineData("/api/patients/{id}/clinical-details", "Get")]
    [InlineData("/api/patients/{patientId}/vitals", "Post")]
    public void Swagger_IncludesXmlSummaryAndResponses(string path, string method)
    {
        using var factory = new CardiacMonitorApiFactory();
        var document = factory.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");
        var operation = document.Paths.Single(p =>
            string.Equals(p.Key, path, StringComparison.OrdinalIgnoreCase))
            .Value.Operations.Single(p => p.Key.ToString() == method).Value;
        Assert.False(string.IsNullOrWhiteSpace(operation.Summary));
        Assert.True(operation.Responses.Count >= 2);
    }

    [Theory]
    [InlineData("/api/auth/register", "200")]
    [InlineData("/api/auth/login", "200")]
    [InlineData("/api/patients/{patientId}/vitals", "201")]
    public void Swagger_IncludesRequestAndResponseExamples(string path, string status)
    {
        using var factory = new CardiacMonitorApiFactory();
        var document = factory.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");
        var operation = document.Paths.Single(p =>
            string.Equals(p.Key, path, StringComparison.OrdinalIgnoreCase))
            .Value.Operations[Microsoft.OpenApi.Models.OperationType.Post];
        Assert.NotNull(operation.RequestBody.Content["application/json"].Example);
        Assert.NotNull(operation.Responses[status].Content["application/json"].Example);
        Assert.NotNull(operation.Responses[status].Content["application/json"].Schema);
    }

    [Fact]
    public void FinalPostmanCollection_CoversEverySwaggerOperation_WithTestsAndExamples()
    {
        using var factory = new CardiacMonitorApiFactory();
        var document = factory.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");
        var root = factory.Services.GetRequiredService<IWebHostEnvironment>().ContentRootPath;
        using var collection = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            root, "postman", "CardiacMonitor.Final.postman_collection.json")));
        var items = collection.RootElement.GetProperty("item").EnumerateArray()
            .SelectMany(folder => folder.GetProperty("item").EnumerateArray()).ToList();
        var documented = new HashSet<string>();
        foreach (var item in items)
        {
            var request = item.GetProperty("request");
            var path = request.GetProperty("url").GetString()!
                .Replace("{{baseUrl}}", "").Split('?')[0];
            Assert.True(documented.Add(request.GetProperty("method").GetString() + " " + Normalize(path)));
            var example = Assert.Single(item.GetProperty("response").EnumerateArray());
            var code = example.GetProperty("code").GetInt32();
            var script = item.GetProperty("event").EnumerateArray()
                .Single(e => e.GetProperty("listen").GetString() == "test")
                .GetProperty("script").GetProperty("exec").EnumerateArray()
                .Select(line => line.GetString());
            Assert.Contains(script, line => line!.Contains($"to.have.status({code})"));
        }
        var expected = document.Paths.SelectMany(path => path.Value.Operations.Select(
            operation => operation.Key.ToString().ToUpperInvariant() + " " + Normalize(path.Key)))
            .ToHashSet();
        Assert.Equal(39, expected.Count);
        Assert.True(expected.SetEquals(documented), "Postman routes must match the live Swagger operation inventory.");
    }

    private static string Normalize(string path) => Regex.Replace(
        path, @"\{\{[^}]+\}\}|\{[^}]+\}", "{id}").ToLowerInvariant();
}
