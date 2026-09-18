using CardiacMonitor.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace CardiacMonitor.IntegrationTests;

// Exposes request-local SQL counts only in the integration-test host.
internal sealed class QueryCountStartupFilter : IStartupFilter
{
    public const string HeaderName = "X-Test-Database-Query-Count";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                context.Response.OnStarting(() =>
                {
                    var count = context.Items.TryGetValue(
                        DatabaseQueryCounterInterceptor.QueryCountItemKey,
                        out var value) && value is int queries ? queries : 0;
                    context.Response.Headers[HeaderName] = count.ToString();
                    return Task.CompletedTask;
                });
                await nextMiddleware();
            });
            next(app);
        };
    }
}
