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
    )
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
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

var random = new Random();

app.MapGet("/randomuser", async (ILogger<Program> logger) => {
    var delay = random.Next(1, 10_000);
    logger.LogInformation($"delaying for {delay}ms");
    await Task.Delay(delay);
    logger.LogWarning("random user not found");
    return Results.NotFound();
})
.WithName("GetRandomUser")
.WithOpenApi();

app.MapGet("/user", (ILogger<Program> logger, [FromQuery(Name = "id")] int id) => {
    logger.LogInformation($"user has been called for user id {id}");
    return Results.Ok(new User("hello", "world"));
})
.WithName("GetUserById")
.WithOpenApi();

app.MapPost("/user", (ILogger<Program> logger, [FromBody] User user) => {
    if (string.IsNullOrWhiteSpace(user.Name)){
        throw new ArgumentNullException("user.Name is null or empty");
    }
    logger.LogInformation($"adding user name: {user.Name}");
    return Results.Created();
})
.WithName("CreateUser")
.WithOpenApi();

app.Run();

record User(string Name, string Surname){

}