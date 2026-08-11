using BITask.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ---- Services ----
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// FRONTEND: lets the Angular dev server (ng serve, localhost:4200) call this gateway from the
// browser. Isolated here on purpose - remove this block plus the frontend/ folder to fully
// revert the frontend work.
const string AngularDevCorsPolicy = "AngularDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// ---- Pipeline ----
// Every request that enters the system through the gateway gets a correlation id and is
// logged here; the same custom middleware is reused unmodified inside each downstream
// microservice, so a single request can be traced end-to-end.
app.UseCustomExceptionHandling();
app.UseCorrelationId();
app.UseRequestLogging();
app.UseCors(AngularDevCorsPolicy);

app.MapGet("/", () => Results.Ok(new
{
    service = "BITask API Gateway",
    routes = new[] { "/api/auth/*", "/api/products/*", "/odata/*", "/health/auth", "/health/products" }
}));

app.MapReverseProxy();

app.Run();
