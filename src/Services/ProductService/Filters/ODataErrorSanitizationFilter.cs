using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProductService.Filters;

/// <summary>
/// OData's [EnableQuery] attribute (e.g. on invalid $filter/$select/$expand) always embeds the
/// full exception type and .NET stack trace in the error body, regardless of environment - see
/// EnableQueryAttribute.CreateErrorResponse in Microsoft.AspNetCore.OData. Strip that down to
/// just the human-readable message outside Development so internals aren't leaked to clients.
/// </summary>
public class ODataErrorSanitizationFilter : IResultFilter
{
    private readonly IWebHostEnvironment _env;

    public ODataErrorSanitizationFilter(IWebHostEnvironment env)
    {
        _env = env;
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (_env.IsDevelopment())
        {
            return;
        }

        if (context.Result is ObjectResult { Value: SerializableError original } result)
        {
            var sanitized = new SerializableError();
            if (original.TryGetValue("Message", out var message))
            {
                sanitized["Message"] = message;
            }

            result.Value = sanitized;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
