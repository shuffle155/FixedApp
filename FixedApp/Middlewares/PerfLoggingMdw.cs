using System.Diagnostics;

namespace FixedApp.Middlewares
{
    public class PerfLoggingMdw
    {
        private readonly RequestDelegate next;

        public PerfLoggingMdw(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            await next(context);
            stopwatch.Stop();
            double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
            if (context.Request.Path.Value != null)
            {
                string logLine = String.Format("{0}, " +
                    "{1}, {2}, {3} ms\n", context.Request.Method,
                    context.Request.Path, context.Response.StatusCode,
                    elapsedMs);
                await File.AppendAllTextAsync("performance_log.csv", logLine);
            }
        }
    }
}