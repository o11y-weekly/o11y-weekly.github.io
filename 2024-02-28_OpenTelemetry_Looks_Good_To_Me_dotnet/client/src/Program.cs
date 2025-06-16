using System.Net;
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
    {
        logging.IncludeFormattedMessage = true;
        logging.AddOtlpExporter();
    })
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
        .AddHttpClientInstrumentation()
        .AddProcessInstrumentation()
        .AddMeter("custom.*")
        .AddView("custom.*", new ExplicitBucketHistogramConfiguration { Boundaries = [0.0, 0.00025, 0.0005, 0.00075, 0.001, 0.0025, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1] })
        .AddOtlpExporter()
        .AddConsoleExporter()
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

var RANDOM = new Random();

var METER = new System.Diagnostics.Metrics.Meter("custom.service", version: "1.0.0", tags:[new KeyValuePair<string, object?>("hello", "world")]);

var HISTO = METER.CreateHistogram<double>("custom.http.duration", "s", "custom http duration histogram", tags:[new KeyValuePair<string, object?>("tag.histo", "tag.histo.value")]);

var getUser = async (ILogger<Program> logger, int id) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    try
    {
        logger.LogInformation("calling GetUser '{Id}'", id);

        logger.LogInformation("calling GetUser {Id}", id);

        logger.LogInformation("calling GetUser {id}", id);
        var user = await ServiceClient.GetUser(id);
        if (user == null)
        {
            logger.LogWarning("user {id} not found", id);
            return Results.NotFound();
        }
        var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return Results.Ok($"User: {user.Name} {user.Surname} {ts}");
    }
    finally
    {
        HISTO.Record(sw.Elapsed.TotalSeconds, KeyValuePair.Create<string, object?>("queue.url", "queue url"));
    }
};

app.MapGet("/randomuser", async (ILogger<Program> logger) =>
{
    return await getUser(logger, RANDOM.Next(1000, 1500));
})
.WithName("GetRandomUser")
.WithOpenApi();

app.MapGet("/user", async (ILogger<Program> logger, [FromQuery(Name = "id")] int id) =>
{
    return await getUser(logger, id);
})
.WithName("GetUser")
.WithOpenApi();

app.MapPost("/user", async (ILogger<Program> logger, [FromBody] User user) =>
{
    logger.LogInformation("adding user name: {}", user.Name);
    return await ServiceClient.CreateUser(user);
})
.WithName("CreateUser")
.WithOpenApi();

app.Run();

static class ServiceClient
{
    private static readonly HttpClient httpClient = HttpClient();

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines
    /// </summary>
    /// <returns></returns>
    private static HttpClient HttpClient()
    {
        return new HttpClient();
    }

    public static async Task<User?> GetUser(int id)
    {
        using var response = await httpClient.GetAsync($"http://service:8080/user?id={id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<User>();
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        var content = await response.Content.ReadAsStringAsync();
        throw new ApplicationException($"internal server error: {content}");
    }

    public static async Task<IResult> CreateUser(User user)
    {
        using var response = await httpClient.PostAsJsonAsync("http://service:8080/user", user);
        response.EnsureSuccessStatusCode();
        return Results.Created();
    }

}
record User(string Name, string Surname)
{

}