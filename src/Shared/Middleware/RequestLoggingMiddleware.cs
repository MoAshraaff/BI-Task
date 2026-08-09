using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BITask.Shared.Middleware;

/// <summary>
/// Custom middleware that logs method, path, status code and elapsed time for every request
/// handled by the pipeline it is registered in.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("--> {Method} {Path}{QueryString}", context.Request.Method, context.Request.Path, context.Request.QueryString);

        await _next(context);

        stopwatch.Stop();
        _logger.LogInformation(
            "<-- {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
