# 2023-11-30 #7 What is OpenTelemetry ?

It has been 14 years since Google created Dapper to monitor distributed system.

What is OpenTelemetry? Where does it come from? What are its influences ? Finally is it really mature ?

But first, what is the history of distributed tracing ?

## History of distributed tracing

- 2010: [Dapper](https://research.google/pubs/pub36356/)
- 2012: [Zipkin](https://www.uber.com/en-FR/blog/distributed-tracing/) from uber
- 2015: [Jaeger](https://www.jaegertracing.io/) tracing
- 2015/11: [opentracing spec first commit](https://github.com/opentracing/opentracing-go/commit/eab1a36e622e49f29d348dc39bc03730ae228b72)
- 2016: [Towards Turnkey Distributed Tracing post](https://medium.com/opentracing/towards-turnkey-distributed-tracing-5f4297d1736)
- 2017/01: [opencensus instrumentation first commit](https://github.com/census-instrumentation/opencensus-go/commit/304ea252d1c39e8aecc84d1bb608c806ff25bfb3)
- 2019/05 : [opencensus](https://opencensus.io/) jaeger tracing, [opentracing](https://github.com/opentracing/opentracing-go) merged to [opentelemetry spec + instrumentation](https://github.com/open-telemetry/opentelemetry-java/pull/244)

Back in 2017, many other vendors (mostly cloud providers) provided such feature like Azure with the service map (with azure function FAAS) to vizualise service dependencies including statistics, metrics and sampling of errors and latencies.

The aim of OpenTelemetry is to provide a specification, instrumentation, exporter, collectors (agent) and backend specification.

OpenTelemetry is a [CNCF project](https://www.cncf.io/) and claims [vendor- and tool-agnostic (backend-agnostic)](https://opentelemetry.io/docs/what-is-opentelemetry/).

It starts from the instrumentation of the observed application, exported to optional collectors which flush data to backend such [Jaeger](https://www.jaegertracing.io/), [Tempo](https://grafana.com/oss/tempo/), [Prometheus](https://prometheus.io/), [Mimir](https://grafana.com/oss/mimir/), [Loki](https://grafana.com/oss/loki/), ...

All the communication between observed application to telemetry backend uses the same protocol : [OTLP](https://github.com/open-telemetry/opentelemetry-proto/tree/main/opentelemetry/proto)

## Signals
Reference: https://opentelemetry.io/docs/concepts/signals/

Signal and telemetry are synonyms, metrics, logs, traces and the very latest one (or the old new): profiling. 

### Metrics

Metrics are exported at a configured rate, depending the resolution, from 10s to 60s means respectively from 6 to 1 metric per minute. The higher the resolution the higher metric/datapoint per minute is handled by the backend.

Another important aspect of metrics is how the value is collected and aggregated. It can be cumulative or delta.
A dedicated post has been made before: [Cumulative and delta Temporality](../2023-11-09_Monotonicity/README.md)

### Logs
Logs can be correlated to traces or simply correlated because the system is distributed and from the client point of view, the transaction traverses to multiple apps.

It is really handy to see all logs for a given transaction inside the distributed system like if the system was only one instance.

Existing logging libraries can support traceId and spanId context but the official OpenTelemetry log instrumentation provides the context propagation which is very simple to use without too much configuration for many languages.

Pay clause attention that existing library might provide thread static context propagation which conflict with new threading model (green thread, virtual task) including java and dotnet. This is why the OpenTelemetry context propagation should be used in favor of such thread static method.

OTLP started with traces then metrics and logs was the latest mature signal integrated in OpenTelemetry which is really different from other solution because usually backend and solutions start by integrating log first.

[By understanding the OpenTelemetry history](./README.md#history-of-distributed-tracing), it is not that hard to understand that log is the least mature instrumentation because specification and implementation started to solve distributed tracing and metrics.

### Traces

As mentioned in [logs](./README.md#logs) OpenTelemetry started with this signal to solve distributed tracing.

A transaction of a client can traverse multiple components and servers in a system and analysing what happen in a distributed system can be hard.

To solve this issue, OpenTelemetry provides specifications, instrumentation and exporters to collect traces and export them to backends without too much effort.

Instrumentation can be manual or automatic.

The automatic instrumentation can be really verbose due too the number of framework used in the observed application and testing the output of such configuration before pushing the conf directly to production is a good idea to avoid hammering the backend or simply having high and large traces which might not be useful.

### Profiling
Profiling is the old new signal since before OpenTelemetry or simply observability tools exist, profiling was still there to solve performance issues.

Profiling can be scoped to the runtime (dotnet, java, golang, ...) or system (flamegraph). Depending the situation, a good code can hammer the system and a profiling with system calls really help to get the problem.

A dedicated post will be available later for the profiling part.

All this signals should be specified and a protocol should support serialization/deserialization and transport of those signals which is the aim of OTLP

## OTLP

gRPC has been chosen from OpenTelemetry for performance, tooling and specification reason.

[Protobuf](https://protobuf.dev/) (protocol buffer 3) is a specification and toolchain to make message, service and client/backend.

An excellent medium post explains this perfectly why json and REST would not be a good choice over gRPC : https://medium.com/data-science-community-srm/json-is-incredibly-slow-heres-what-s-faster-ca35d5aaf9e8

/!\ OTLP does not use protocol buffer streaming at all. Another post will be done later on this topic about missed opportunities. Pay close attention that large payload or long workflow cannot be integrated in OTLP directly. Another usecase, for large logs cannot fit in such configuration since all the data should be sent at once causing large memory allocation on backend and collectors.

### gRPC

gRPC is simply using protocol buffer message and service specification over HTTP2.

A HTTP1.X can be supported by using a body encoded with protobuf. OTLP supports both http2 and http protocol for the transport but address can be different in the configuration.

For legacy and compatibility support, [envoy](https://www.envoyproxy.io/docs/envoy/latest/configuration/http/http_filters/grpc_json_transcoder_filter) can be used to transcode gRPC <> REST for free. [Envoy](https://www.envoyproxy.io/) is largely used in observability for service mesh purpose but also be a good candidate in front of legacy infrastructure.

## Instrumentation
Reference: https://opentelemetry.io/docs/concepts/instrumentation/

Instrumentation can be manual or automatic and refers to the act of decorating/collecting information to export it for later usage.

The easiest instrumentation is the manual log instrumentation which is not hard to understand. A log library should be used to log events inside a function for diagnostic purpose. The log can be written in a file which is scrapped or analyzed later.

In this log context, writing code around the function to extract some useful information to help futur diagnostic is a manual Instrumentation.

When using a webserver, logging http events can be really useful for debuging purpose and doing it manually can be error prone and unefficient.

To solve this, automatic instrumentation can be done by using framework integration depending the language.

This instrumentation can also produce metrics and traces because [doing everything with log can be an antipattern](../2023-10-18_What_is_not_an_observability_solution/What_is_not_an_o11y_solution.md#all-you-need-is-logs).

/!\ There is no magic, instrumentation collects and aggregate data, consumes CPU and Memory with an overhead. Also, if the storage used is the memory, which is good for low overhead, it means that all signals are lost when the process panic. Such solutions are best effort. For instance, SLA can be different between signals and logs can be flushed to the disk (and even synchronously or asynchonously depending SLA) while metrics and traces might not.

### Manual

Manual instrumentation should be considered when the observed code does not come from a standard library / open source project / framework.

Since everything cannot be tracked automatically, it is a good thing to master first instrumentation, exporters and collectors on logs and metrics first.It is different for traces, starting from automatic instrumentation and filter can be a good choice to start quickly.

Distributed tracing and traces is the most mature signal integrated in OpenTelemetry since it was the first signal integrated in the solution. Automatic instrumentation can be done really easily and depending the number of backend and framework, the traces can be really consise with automatic instrumentation.

A good sign of the technical debt of the observed solution can be the number of spans and framework used to handle one transaction. Application with too much frameworks can produces heavy and large traces which is not good for the observability backend.

### Automatic
Reference: https://opentelemetry.io/docs/concepts/components/#automatic-instrumentation

Automatic instrumentation decorates the code dynamically or statically depending the language.

A majority of framework are supported, for each language, an automatic instrumentation is available and a list of compatible framework is also available.

Before using automatic instrumentation, a list of used framework should be done to compare between requirements and available framework. If the framework is not available, a [manual instrumentation](./README.md#manual) should be used.

As an example, in [dotnet](https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation), instrumentation is setup through a [startup hook](https://github.com/dotnet/runtime/blob/main/docs/design/features/host-startup-hook.md). This [OpenTelemetry startup hook](https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/blob/main/src/OpenTelemetry.AutoInstrumentation.StartupHook/StartupHook.cs) inject [native code](https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/tree/main/src/OpenTelemetry.AutoInstrumentation.Native) to integrate with the CLR.

In java, a [java agent](https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/) can be used which contains instrumentation and exporter.

In rust, OpenTelemetry does not mention automatic but still, a list of well know frameworks and [example](https://github.com/open-telemetry/opentelemetry-rust/tree/main/examples/tracing-http-propagator) are available and implement instrumentation for. Macro are used to statically, during the build part, to produce the instrumentation while exporters can be configured in the main entrypoint.

The rust version is less magic and such integration offers a better control over performance overhead of the instrumentation.

### Alternatives
An alternative of instrumented code can be using external distributed tracing to decorate legacy app by using a proxy. [Envoy](https://www.envoyproxy.io/docs/envoy/latest/start/sandboxes/opentelemetry) can be used to support opentelemetry widely on a huge legacy to offer the distributed tracing between application without having deep details. It can be a good start to have the global overview to find out which service is the less effective first without too much integration effort.

## Exporters
Reference: https://opentelemetry.io/docs/concepts/components/#exporters

Usually instrumentations libraries are packed with exporters but can be used separately.

The exporter, like for the [previous log use case](./README.md#instrumentation) used to flush the aggregated data to a collector.

The exporter should care about the interval, the higher the interval, the less request can be sent to the backend but the higher the risk of lost signal might occurs.

To have a good balance between risk / signal resolution, a local agent (OpenTelemetry collector) is useful to flush as soon as possible the telemetry without hammering the backend.

## Collector
Reference: https://opentelemetry.io/docs/concepts/components/#collector

> "The OpenTelemetry Collector is a vendor-agnostic proxy that can receive, process, and export telemetry data. It supports receiving telemetry data in multiple formats (for example, OTLP, Jaeger, Prometheus, as well as many commercial/proprietary tools) and sending data to one or more backends. It also supports processing and filtering telemetry data before it gets exported."

A collector supports the service definition of OpenTelemetry.

Collectors can support a high number of receivers and exporters but also the main protocol OTLP.

## Sampler

Reference: https://opentelemetry.io/docs/concepts/sampling/

Sampler is mostly used for tracing since all traces are not relevant and without a sampling strategy, 100% of the telemetry is sent to the backend with high cost and usage.

To avoid sending useless telemetry, a sampling strategy can help to reduce the amount of telemetry data.

### Head sampling

The head sampling means that the decision of keeping the telemetry is decided ahead of time.

[Probabilistic sampling](https://opentelemetry.io/docs/specs/otel/trace/tracestate-probability-sampling/#consistent-probability-sampling) can be done without any collector since the decision is taken from the start and transported in the context via [context propagation baggage](./README.md#cross-service-propagators).

The head sampling helps to keep the instrumentation space in the observed application small without too much overhead. Setting 100% of instrumentation might hurt the overall performance at some point but not that high in general.

Keeping 10% of traces can be useful at the beginning but it will not help in case of p99 optimization. In such configuration, the head sampling will not help.

### Tail sampling

OpenTelemetry Collector provides a [tail sampling](https://opentelemetry.io/docs/concepts/sampling/#tail-sampling) processor : [tailsamplingprocessor](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/processor/tailsamplingprocessor) which as opposed to head sampling does not decide ahead of time but after an event in a windows period to keep the telemetry.

Errors and high latencies can be kept to have meaningful full traces without flushing all the useless part (goodput).

## Cross service Propagators
Reference: https://opentelemetry.io/docs/concepts/components/#cross-service-propagators

Technically, the trace id, decision and all other context should be transfered from one application to another.

To do it, process to process context propagation is done inside the instrumentation and stored via OTLP and [baggage](https://opentelemetry.io/docs/concepts/signals/baggage/).

It is generally metadata associated with the telemetry payload.

Instrumentation should retrieve the context, extract information such trace id to correlate span together.

The OpenTelemetry [Baggage](https://opentelemetry.io/docs/concepts/signals/baggage/) documentation explains the use case really well.

## Conclusion

In conclusion, OpenTelemetry is mature and the collector contrib [Open telemetry collector contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib/) is helpful to integrate existing or legacy application.

Depending signals and telemetry, the maturity can depend and a common trap is thinking that OpenTelemetry logs are more mature than others which is false.

Again, doing everything with one signals, might hurt the required SLA and depending the signal and volume the complexity to support a given SLA for a backend responsible of storing one signal kind can be totally different.

OTLP does not use the streaming feature of gRPC and protocol buffer which is discussable or incompatible with scenario like large logs, ...

OpenTelemetry is a good standard and define perfectly common observability use case already explained in [a previous post](../2023-10-18_What_is_not_an_observability_solution/What_is_not_an_o11y_solution.md)