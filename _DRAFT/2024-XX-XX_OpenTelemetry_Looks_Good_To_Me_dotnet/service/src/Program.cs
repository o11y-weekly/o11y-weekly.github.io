using Microsoft.AspNetCore.Mvc;
using Npgsql;
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
    }
)
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
        .AddOtlpExporter()
    )
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddNpgsql()
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

var COUNTER = 0;

var MIN_LATENCY = int.Parse(Environment.GetEnvironmentVariable("MIN_LATENCY") ?? "0");
var MAX_LATENCY = int.Parse(Environment.GetEnvironmentVariable("MAX_LATENCY") ?? "0");
var LATENCY_RATIO = double.Parse(Environment.GetEnvironmentVariable("LATENCY_RATIO") ?? "0");
var FAILURE_RATIO = double.Parse(Environment.GetEnvironmentVariable("FAILURE_RATIO") ?? "0");

var isEnabled = (int counter, double ratio) => counter % (1 / ratio) == 0;

var callSlowDependency = async (int counter, int delay) =>
{
    if (isEnabled(counter, LATENCY_RATIO))
    {
        await Task.Delay(delay);
    }
};

app.MapGet("/user", async (ILogger<Program> logger, [FromQuery(Name = "id")] int id) =>
{
    var counter = COUNTER;
    Interlocked.Increment(ref COUNTER);

    logger.LogInformation("counter value: {counter}", counter);

    var delay = RANDOM.Next(MIN_LATENCY, MAX_LATENCY);
    await callSlowDependency(counter, delay);

    if (isEnabled(counter, FAILURE_RATIO))
    {
        await DataBaseService.GetFailedUserFromDatabase(id);
    }

    logger.LogInformation("user has been called for user id {id}", id);
    var user = await DataBaseService.GetUser(id);
    if (user == null)
    {
        logger.LogWarning("user {id} not found in database", id);
        return Results.NotFound();
    }
    logger.LogInformation("user {}: {}, found", id, user.Name);
    return Results.Ok(user);
})
.WithName("GetUserById")
.WithOpenApi();

app.MapPost("/user", async (ILogger<Program> logger, [FromBody] User user) =>
{
    if (string.IsNullOrWhiteSpace(user.Name))
    {
        throw new ArgumentNullException("user.Name is null or empty");
    }
    logger.LogInformation("adding user name: {}", user.Name);
    await DataBaseService.CreateUser(user);
    return Results.Created();
})
.WithName("CreateUser")
.WithOpenApi();

app.Run();

record User(string Name, string Surname)
{

}

static class DataBaseService
{
    static readonly string connString = "Host=db;Username=postgres;Password=ReallyBadPassword;Database=persons";
    static readonly NpgsqlConnection conn = new NpgsqlDataSourceBuilder(connString).Build().OpenConnection();

    public static async Task<User?> GetUser(int id)
    {
        await using var cmd = new NpgsqlCommand("select firstname, surname from persons where id=@id", conn);
        cmd.Parameters.AddWithValue("id", NpgsqlTypes.NpgsqlDbType.Integer, id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var firstName = reader.GetString(0);
            var surname = reader.GetString(1);
            return new User(firstName, surname);
        }
        return null;
    }

    public static async Task<User?> GetFailedUserFromDatabase(int id)
    {
        await using var cmd = new NpgsqlCommand("select firstname, surname from bad_table where id=@id", conn);
        cmd.Parameters.AddWithValue("id", NpgsqlTypes.NpgsqlDbType.Integer, id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var firstName = reader.GetString(0);
            var surname = reader.GetString(1);
            return new User(firstName, surname);
        }
        return null;
    }

    public static async Task<int> CreateUser(User user)
    {
        await using var cmd = new NpgsqlCommand("insert into persons(firstname, surname) values(@firstname, @surname)", conn);
        cmd.Parameters.AddWithValue("firstname", user.Name);
        cmd.Parameters.AddWithValue("surname", user.Surname);
        return await cmd.ExecuteNonQueryAsync();
    }
}