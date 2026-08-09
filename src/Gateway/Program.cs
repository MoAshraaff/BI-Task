using BITask.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ---- Services ----
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// ---- Pipeline ----
// Every request that enters the system through the gateway gets a correlation id and is
// logged here; the same custom middleware is reused unmodified inside each downstream
// microservice, so a single request can be traced end-to-end.
app.UseCustomExceptionHandling();
app.UseCorrelationId();
app.UseRequestLogging();

app.MapGet("/", () => Results.Ok(new
{
    service = "BITask API Gateway",
    routes = new[] { "/api/auth/*", "/api/products/*", "/odata/*", "/health/auth", "/health/products" }
}));

app.MapReverseProxy();

app.Run();
