using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CardiacMonitor.Infrastructure;

public sealed class DatabaseQueryCounterInterceptor : DbCommandInterceptor
{
    public const string QueryCountItemKey = "DatabaseQueryCount";
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Stores access to the current request so executed commands can be counted.
    public DatabaseQueryCounterInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Counts a completed query that returned rows.
    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        IncrementQueryCount();
        return result;
    }

    // Counts a completed asynchronous query that returned rows.
    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        IncrementQueryCount();
        return ValueTask.FromResult(result);
    }

    // Counts a completed scalar database command such as COUNT.
    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        IncrementQueryCount();
        return result;
    }

    // Counts a completed asynchronous scalar database command.
    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        IncrementQueryCount();
        return ValueTask.FromResult(result);
    }

    // Increments the request-local database command counter.
    private void IncrementQueryCount()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        var currentCount = httpContext.Items.TryGetValue(
            QueryCountItemKey,
            out var value) && value is int count
                ? count
                : 0;
        httpContext.Items[QueryCountItemKey] = currentCount + 1;
    }
}
