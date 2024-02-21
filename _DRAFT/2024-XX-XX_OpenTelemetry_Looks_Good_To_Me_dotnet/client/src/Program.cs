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
    logger.LogInformation("delaying for {delay}ms", delay);
    await Task.Delay(delay);
    logger.LogWarning("random user not found");
    return Results.NotFound();
})
.WithName("GetRandomUser")
.WithOpenApi();

app.MapGet("/user", async (ILogger<Program> logger, [FromQuery(Name = "id")] int id) => {
    logger.LogInformation("calling GetUser {id}", id);
    var user = await ServiceClient.GetUser(id);
    if (user == null) {
        logger.LogWarning("user {id} not found", id);
        return Results.NotFound();
    }
    var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    return Results.Ok($"User: {user.Name} {user.Surname} {ts}");
})
.WithName("GetUser")
.WithOpenApi();

app.MapPost("/user", async (ILogger<Program> logger, [FromBody] User user) => {
    logger.LogInformation("adding user name: {}", user.Name);
    return await ServiceClient.CreateUser(user);
})
.WithName("CreateUser")
.WithOpenApi();

app.Run();

static class ServiceClient {
    private static readonly HttpClient httpClient = new();

    public static async Task<User?> GetUser(int id) {
        return await httpClient.GetFromJsonAsync<User>($"http://service:8080/user?id={id}");
    }

    public static async Task<IResult> CreateUser(User user) {
        var response = await httpClient.PostAsJsonAsync("http://service:8080/user", user);
        response.EnsureSuccessStatusCode();
        return Results.Created();
    }

}
record User(string Name, string Surname){

}