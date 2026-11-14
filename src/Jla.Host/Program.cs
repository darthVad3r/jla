// Composition root. Increment 0 hosts only a health endpoint.
// The jla command line will be a command mode of this project (a thin client of the API), not a separate project.
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
