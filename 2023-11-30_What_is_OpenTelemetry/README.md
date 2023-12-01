# 2023-11-30 #7 What is OpenTelemetry ?

It has been 14 years since Google created Dapper to monitor distributed system.

What is OpenTelemetry? Where does it come from? What are its influences ? Finally is it really mature ?

## History

- 2010: [Dapper](https://research.google/pubs/pub36356/)
- 2012: [Zipkin](https://www.uber.com/en-FR/blog/distributed-tracing/)
- 2015: [Jaeger](https://www.jaegertracing.io/)
- 2015/11: [opentracing spec first commit](https://github.com/opentracing/opentracing-go/commit/eab1a36e622e49f29d348dc39bc03730ae228b72)
- 2016 [Towards Turnkey Distributed Tracing post](https://medium.com/opentracing/towards-turnkey-distributed-tracing-5f4297d1736)
- 2017/01: [opencensus instrumentaion first commit](https://github.com/census-instrumentation/opencensus-go/commit/304ea252d1c39e8aecc84d1bb608c806ff25bfb3)
- 2019/05 : [opencensus](https://opencensus.io/) jaeger tracing, [opentracing](https://github.com/opentracing/opentracing-go) merged to [opentelemetry spec + instrumentation](https://github.com/open-telemetry/opentelemetry-java/pull/244)

Back in 2017, many other competitors provided such feature like Azure with the service map (with azure function FAAS) to vizualise service dependencies including statistics, metrics and sampling of errors and latencies.

The aim of OpenTelemetry is to provide a specification, instrumentation, exporter, collectors (agent) and backend specification.

OpenTelemetry is a [CNCF project](https://www.cncf.io/) and claims [vendor- and tool-agnostic (backend-agnostic)](https://opentelemetry.io/docs/what-is-opentelemetry/).

It starts from the instrumentation of the observed application, exported to optional collectors which flush data to backend such [Jaeger](https://www.jaegertracing.io/), [Tempo](https://grafana.com/oss/tempo/), [Prometheus](https://prometheus.io/), [Mimir](https://grafana.com/oss/mimir/), [Loki](https://grafana.com/oss/loki/), ...

All the communication between observed application to telemetry backend uses the same protocol : [OTLP](https://github.com/open-telemetry/opentelemetry-proto/tree/main/opentelemetry/proto)

## Signals
Reference: https://opentelemetry.io/docs/concepts/signals/

Signal and telemetry are synonyms, metrics, logs, traces and the very latest one (or the old new): profiling. 

### Metrics

Metrics are exported at a configured rate, depending the resolution, from 10s to 60S means respectively from 6 to 1 metric per minute. The higher the resolution the higher metric/datapoint per minute should be handled by the backend.

Another important aspect of metrics is how the value is collected and aggregated. It can be cumulative or delta.
A dedicated post has been made before: [Cumulative and delta Temporality](../2023-11-09_Monotonicity/README.md)

### Logs
Logs can be correlated to traces or simply correlated because the system is distributed and from the client point of view, the transaction span multiple apps.

It is really handy to see all logs for a given transaction inside the distributed system like if the system was only one instance.

Existing logging library can support traceId and spanId context but the official OpenTelemetry log instrumentation provides the context propagation which is very simple to use without too much configuration for many languages.

OTLP started with traces then metrics and logs was the latest mature signal integrated in OpenTelemetry which is really different from other solution because usually backend and solutions start by integrating log first.

[By understanding the OpenTelemetry history](./README.md#history), it is not that hard to understand that log is the least mature instrumentation because specification and implementation started to solve distributed tracing and metrics.

### Traces

As mentioned in [logs](./README.md#logs) OpenTelemetry started with this signal to solve distributed tracing.

A transaction of a client can traverse multiple component and servers in a system and analysing what happen in a distributed system can be hard.

To solve this issue, OpenTelemetry provides specifications, instrumentation and exporters to collect traces and export them to backends without too much effort.

Instrumentation can be manual or automatic.

The automatic instrumentation can be really verbose due too the number of framework used in the observed application and testing the output of such configuration before pushing the conf directly to production is a good idea to avoid hammering the backend or simply having high and large traces which might not be useful.

### Profiling
Profiling is the old new signal since before OpenTelemetry or simply observability tools exist, profiling was still there to solve performance issues.

Profiling can be scoped to the runtime (dotnet, java, golang, ...) or system (flamegraph). Depending the situation, a good code can hammer the system and a profiling with system calls.

A dedicated post will be available later for the profiling part.


All this signals should be specified and a protocol should support serialization/deserialization and transport of those singals which is the aim of OTLP

## OTLP

gRPC has been chosen from OpenTelemetry for performance, tooling and specification reason.

[Protobuf](https://protobuf.dev/) (protocol buffer 3) is a specification and toolchain to make message service and client/backend.

An excellent medium post explains this perfectly why json and REST would not be a good choice over gRPC : https://medium.com/data-science-community-srm/json-is-incredibly-slow-heres-what-s-faster-ca35d5aaf9e8

/!\ The thing is that OTLP does not use protocol buffer streaming at all which at some point can be bad. Another post will be done later on this topic and missed opportunities but pay close attention that large payload or long workflow cannot be integrated in OTLP directly. Another usecase, for large logs cannot fit in such configuration since all the data should be sent at once causing large memory allocation on backend and collectors.

### gRPC

gRPC is simply using protocol buffer message and service specification over HTTP2.

A HTTP1.X can be supported by using a body encoded with protobuf. OTLP supports both http2 and http protocol for the transport but address can be different in the configuration.

For legacy and compatibility support, [envoy](https://www.envoyproxy.io/docs/envoy/latest/configuration/http/http_filters/grpc_json_transcoder_filter) can be used to transcode gRPC <> REST for free. [Envoy](https://www.envoyproxy.io/) is largely used in observability for service mesh purpose but also be a good candidate in front of legacy infrastructure.

## Instrumentation
Reference: https://opentelemetry.io/docs/concepts/instrumentation/

Instrumentation can be manual or automatic and refers to the act of decorating/collecting information to export it for later usage.

The easiest instrumentation is the manual log instrumentation which is not hard to understand. A log library should be used to log event inside a function for diagnostic purpose. The log can be written in a file which is scrapped or analyzed later.

In this log context, writing code around the function to extract some useful information to help futur diagnostic is a manual Instrumentation.

When using a webserver, logging http events can be really useful for debuging purpose and doing it manually can be error prone and unefficient.

To solve this, automatic instrumentation can be done by using framework integration depending the language.

This instrumentation can also produce metrics and traces because [doing everything with log can be an antipattern](../2023-10-18_What_is_not_an_observability_solution/What_is_not_an_o11y_solution.md#all-you-need-is-logs).

/!\ There is no magic, instrumentation collects and aggregate data, consumes CPU and Memory with an overhead. Also, if the storage used is the memory, which is good for low overhead, it means that all signals are lost when the process panic. Such solutions are best effort. For instance, SLA can be different between signals and logs can be flushed to the disk (and even synchronously or asynchonously depending SLA) while metrics and traces might not.

### Manual

Manual instrumentation should be considered when the observed code does not come from a standard library / open source project / framework.

Since everything cannot be tracked automatically, it is a good thing to master first instrumentation, exporters and collectors on logs and metrics first while with tracing, it can be slighly different.

Distributed tracing and traces is the most mature signal integrated in OpenTelemetry since it was the first signal integrated in the solution. Automatic instrumentation can be done really easily and depending the number of backend and framework, the traces can be really consise with automatic instrumentation.

A good sign of the technical debt of the observed solution can be the number of spans and framework used to handle one transaction. Application with too much framework can produces heavy and large traces which is not good for the observability backend.

### Automatic
Reference: https://opentelemetry.io/docs/concepts/components/#automatic-instrumentation

Automatic instrumentation decorates the code dynamically or statically depending the language.

As an example, in dotnet, instrumentation is done during the JIT phase where native code can be injected one to avoid paying the abstraction everytime.

In java, a java agent can be use which contains instrumentation and exporter.

In rust, macro are used to statically, during the build phase producing the instrumentation while exporters can be configured in the main entrypoint.

The rust version is less magic and such integration offers a better control over performance overhead of the instrumentation.

## Exporters
https://opentelemetry.io/docs/concepts/components/#exporters

## Collector

Collectors can support a high number of receivers and exporters but also the main protocol OTLP.

## Sampler
https://opentelemetry.io/docs/concepts/components/#sampler
### Head sampling
### Tail sampling

## Cross service Propagators
https://opentelemetry.io/docs/concepts/components/#cross-service-propagators

## Components

## Architecture

## Conclusion