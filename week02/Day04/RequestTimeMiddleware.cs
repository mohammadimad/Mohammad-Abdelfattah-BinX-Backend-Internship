using System.Diagnostics;

namespace Day04
{
    public class RequestTimeMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestTimeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
          
            var timer = Stopwatch.StartNew();
            await Task.Delay(1000);
            await Task.Delay(1000);
            await _next(context);

            timer.Stop();
            var path = context.Request.Path;
            var method = context.Request.Method;
            var elapsedMs = timer.ElapsedMilliseconds;
            Console.WriteLine($"[Request Tracker] {method} {path} took {elapsedMs}ms");
        }
    }
}
