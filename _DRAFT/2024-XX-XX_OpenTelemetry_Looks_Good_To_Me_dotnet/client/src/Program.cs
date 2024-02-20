using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var serviceIndex = builder.Services.FindIndex(x => x.ServiceType == typeof(IMeterFactory));

// var meterFactoryService = builder.Services[serviceIndex];

// Debug.Assert(meterFactoryService.ImplementationType != null);

// var defaultMeterFactory = Activator.CreateInstance(meterFactoryService.ImplementationType) as IMeterFactory;

// Debug.Assert(defaultMeterFactory != null);

builder.Services.RemoveAt(serviceIndex);
builder.Services.AddSingleton<IMeterFactory>(new DefaultMeterFactory());

var s = builder.Services.Where(x => x.ServiceType == typeof(IMeterFactory));
var sm = builder.Metrics.Services.Where(x => x.ServiceType == typeof(IMeterFactory));

Debug.Assert(s.Count() == 1);
Debug.Assert(sm.Count() == 1);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.First<>()

builder
.Services
    .AddMetrics(x =>
    {
        var y = x.EnableMetrics(null);
        y.EnableMetrics(null);

    })
    .AddOpenTelemetry()

    .WithMetrics(opts => opts
        .ConfigureResource(resource => resource.AddService(
        serviceNamespace: "demo",
        serviceName: builder.Environment.ApplicationName,
        serviceVersion: System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
        serviceInstanceId: Environment.MachineName
        ).AddAttributes(new Dictionary<string, object>
        {
            { "deployment.environment", builder.Environment.EnvironmentName },
            { "host.name", Environment.MachineName }
        }))
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        // .AddView(instrument =>
        // {
        //     var type = instrument.GetType();
        //     var tagsProperty = type.GetProperty("Tags");
        //     var setter = tagsProperty?.GetSetMethod(nonPublic:true);


        //     return new MetricStreamConfiguration();
        // })
        .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
            {
                metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
                exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                exporterOptions.Endpoint = new Uri("http://localhost:4317");
                // exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                // exporterOptions.Endpoint = new Uri("http://localhost:4318/v1/metrics");
                // exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                // exporterOptions.Endpoint = new Uri("http://localhost:9009/otlp/v1/metrics");
            }
        )
    );

var s2 = builder.Services.Where(x => x.ServiceType == typeof(IMeterFactory));
var sm2 = builder.Metrics.Services.Where(x => x.ServiceType == typeof(IMeterFactory));


var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

static class EnumerableExtension
{
    public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
    {
        var itemsWithIndices = items.Select((item, index) => new { Item = item, Index = index });
        var matchingIndices =
            from itemWithIndex in itemsWithIndices
            where predicate(itemWithIndex.Item)
            select (int?)itemWithIndex.Index;

        return matchingIndices.FirstOrDefault() ?? -1;
    }
}

internal static class DiagnosticsHelper
{
    /// <summary>
    /// Compares two tag collections for equality.
    /// </summary>
    /// <param name="sortedTags">The first collection of tags. it has to be a sorted List</param>
    /// <param name="tags2">The second collection of tags. This one doesn't have to be sorted nor be specific collection type</param>
    /// <returns>True if the two collections are equal, false otherwise</returns>
    /// <remarks>
    /// This method is used to compare two collections of tags for equality. The first collection is expected to be a sorted array
    /// of tags. The second collection can be any collection of tags.
    /// we avoid the allocation of a new array by using the second collection as is and not converting it to an array. the reason
    /// is we call this every time we try to create a meter or instrument and we don't want to allocate a new array every time.
    /// </remarks>
    internal static bool CompareTags(List<KeyValuePair<string, object?>>? sortedTags, IEnumerable<KeyValuePair<string, object?>>? tags2)
    {
        if (sortedTags == tags2)
        {
            return true;
        }

        if (sortedTags is null || tags2 is null)
        {
            return false;
        }

        int count = sortedTags.Count;
        int size = count / (sizeof(ulong) * 8) + 1;
        BitMapper bitMapper = new BitMapper(size <= 100 ? stackalloc ulong[size] : new ulong[size]);

        if (tags2 is ICollection<KeyValuePair<string, object?>> tagsCol)
        {
            if (tagsCol.Count != count)
            {
                return false;
            }

            if (tagsCol is IList<KeyValuePair<string, object?>> secondList)
            {
                for (int i = 0; i < count; i++)
                {
                    KeyValuePair<string, object?> pair = secondList[i];

                    for (int j = 0; j < count; j++)
                    {
                        if (bitMapper.IsSet(j))
                        {
                            continue;
                        }

                        KeyValuePair<string, object?> pair1 = sortedTags[j];

                        int compareResult = string.CompareOrdinal(pair.Key, pair1.Key);
                        if (compareResult == 0 && object.Equals(pair.Value, pair1.Value))
                        {
                            bitMapper.SetBit(j);
                            break;
                        }

                        if (compareResult < 0 || j == count - 1)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        int listCount = 0;
        using (IEnumerator<KeyValuePair<string, object?>> enumerator = tags2.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                listCount++;
                if (listCount > sortedTags.Count)
                {
                    return false;
                }

                KeyValuePair<string, object?> pair = enumerator.Current;
                for (int j = 0; j < count; j++)
                {
                    if (bitMapper.IsSet(j))
                    {
                        continue;
                    }

                    KeyValuePair<string, object?> pair1 = sortedTags[j];

                    int compareResult = string.CompareOrdinal(pair.Key, pair1.Key);
                    if (compareResult == 0 && object.Equals(pair.Value, pair1.Value))
                    {
                        bitMapper.SetBit(j);
                        break;
                    }

                    if (compareResult < 0 || j == count - 1)
                    {
                        return false;
                    }
                }
            }

            return listCount == sortedTags.Count;
        }
    }
}

internal ref struct BitMapper
{
    private int _maxIndex;
    private Span<ulong> _bitMap;

    public BitMapper(Span<ulong> bitMap)
    {
        _bitMap = bitMap;
        _bitMap.Clear();
        _maxIndex = bitMap.Length * sizeof(ulong) * 8;
    }

    public int MaxIndex => _maxIndex;

    private static void GetIndexAndMask(int index, out int bitIndex, out ulong mask)
    {
        bitIndex = index >> 6; // divide by 64 == (sizeof(ulong) * 8) bits
        int bit = index & (sizeof(ulong) * 8 - 1);
        mask = 1UL << bit;
    }

    public bool SetBit(int index)
    {
        Debug.Assert(index >= 0);
        Debug.Assert(index < _maxIndex);

        GetIndexAndMask(index, out int bitIndex, out ulong mask);
        ulong value = _bitMap[bitIndex];
        _bitMap[bitIndex] = value | mask;
        return true;
    }

    public bool IsSet(int index)
    {
        Debug.Assert(index >= 0);
        Debug.Assert(index < _maxIndex);

        GetIndexAndMask(index, out int bitIndex, out ulong mask);
        ulong value = _bitMap[bitIndex];
        return ((value & mask) != 0);
    }
}

internal sealed class DefaultMeterFactory : IMeterFactory
{
    private readonly Dictionary<string, List<FactoryMeter>> _cachedMeters = new();
    private bool _disposed;

    public DefaultMeterFactory() { }

    public Meter Create(MeterOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.Scope is not null && !object.ReferenceEquals(options.Scope, this))
        {
            throw new InvalidOperationException("InvalidScope");
        }

        Debug.Assert(options.Name is not null);

        lock (_cachedMeters)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DefaultMeterFactory));
            }

            if (_cachedMeters.TryGetValue(options.Name, out List<FactoryMeter>? meterList))
            {
                foreach (Meter meter in meterList)
                {
                    if (meter.Version == options.Version && DiagnosticsHelper.CompareTags(meter.Tags as List<KeyValuePair<string, object?>>, options.Tags))
                    {
                        return meter;
                    }
                }
            }
            else
            {
                meterList = new List<FactoryMeter>();
                _cachedMeters.Add(options.Name, meterList);
            }

            object? scope = options.Scope;
            options.Scope = this;
            var tags = (options.Tags ?? []).Concat([new KeyValuePair<string, object?>("crack", "fric")]); 
            FactoryMeter m = new FactoryMeter(options.Name, options.Version, tags, scope: this);
            options.Scope = scope;

            meterList.Add(m);
            return m;
        }
    }

    public void Dispose()
    {
        lock (_cachedMeters)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (List<FactoryMeter> meterList in _cachedMeters.Values)
            {
                foreach (FactoryMeter meter in meterList)
                {
                    meter.Release();
                }
            }

            _cachedMeters.Clear();
        }
    }
}

internal sealed class FactoryMeter : Meter
{
    public FactoryMeter(string name, string? version, IEnumerable<KeyValuePair<string, object?>>? tags, object? scope)
        : base(name, version, tags, scope)
    {
    }

    public void Release() => base.Dispose(true); // call the protected Dispose(bool)

    protected override void Dispose(bool disposing)
    {
        // no-op, disallow users from disposing of the meters created from the factory.
    }
}