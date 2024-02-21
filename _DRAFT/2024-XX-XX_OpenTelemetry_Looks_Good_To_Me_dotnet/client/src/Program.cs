using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

builder.Services
    .AddResourceMonitoring()
    .AddHealthChecks();

builder
.Logging.AddOpenTelemetry(logging => 
    logging.AddOtlpExporter())
.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(
            serviceName: Environment.GetEnvironmentVariable("SERVICE_NAME") ?? builder.Environment.ApplicationName,
            serviceVersion: System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) // SemVer
            )
        .AddAttributes(new Dictionary<string, object>
            {
                { "host.name", Environment.MachineName }
            })
        )
    .WithMetrics(opts => opts
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter()
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/healthz");

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var random = new Random();

app.MapGet("/randomuser", async (ILogger<Program> logger) => {
    var delay = random.Next(1, 10_000);
    logger.LogInformation($"delaying for {delay}ms");
    await Task.Delay(delay);
    logger.LogWarning("random user not found");
    return Results.NotFound();
});

app.MapGet("/user", (ILogger<Program> logger, [FromQuery(Name = "id")] int id) => {
    logger.LogInformation($"user has been called for user id {id}");
    return Results.Ok($"user {id}");
});

app.MapPost("/user", (ILogger<Program> logger, [FromBody] User user) => {
    if (string.IsNullOrWhiteSpace(user.Name)){
        throw new ArgumentNullException("user.Name is null or empty");
    }
    logger.LogInformation($"adding user name: {user.Name}");
    return Results.Created();
});

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record User(string Name, string Surname){

}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}