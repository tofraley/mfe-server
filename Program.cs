using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddCors();

// Add configuration
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("versions.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add version service
builder.Services.AddSingleton<IVersionResolver, VersionResolver>();

var app = builder.Build();

// Configure middleware
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseSwagger();
app.UseSwaggerUI();

// Version configuration endpoint
app.MapGet("/api/versions", (IVersionResolver resolver) => 
    Results.Ok(resolver.GetVersionsConfig()));

// Version info endpoint
app.MapGet("/api/version/{appName}", (string appName, HttpContext context, IVersionResolver resolver) =>
{
    var version = resolver.ResolveVersion(appName, context);
    return Results.Ok(new { version = version.version, path = version.path });
});

// Serve microfrontend files
app.MapGet("/apps/{appName}/{**path}", (string appName, string path, HttpContext context, IVersionResolver resolver) =>
{
    var (version, basePath) = resolver.ResolveVersion(appName, context);
    var filePath = Path.Combine(basePath, path);

    if (!File.Exists(filePath))
    {
        return Results.NotFound();
    }

    var contentType = MFEServer.Helpers.GetContentType(filePath);
    context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
    context.Response.Headers.Append("X-Version", version);
    
    return Results.File(filePath, contentType);
});

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
