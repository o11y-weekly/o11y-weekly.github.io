# 2023-11-30 #7 What is OpenTelemetry ?

It has been 14 years since Google created Dapper to monitor distributed system.

What is OpenTelemetry? Where does it come from? What are its influences ? Finally is it really mature ?

## History
source : https://devops.com/a-history-of-distributed-tracing/#:~:text=The%20History%20of%20Distributed%20Tracing,open%20source%20distributed%20tracing%20project

- 2010: [Dapper](https://research.google/pubs/pub36356/)
- 2012: [Zipkin](https://www.uber.com/en-FR/blog/distributed-tracing/)
- 2015: [Jaeger](https://www.jaegertracing.io/)
- 2015/11: [opentracing spec first commit](https://github.com/opentracing/opentracing-go/commit/eab1a36e622e49f29d348dc39bc03730ae228b72)
- 2016 [Towards Turnkey Distributed Tracing post](https://medium.com/opentracing/towards-turnkey-distributed-tracing-5f4297d1736)
- 2017/01: [opencensus instrumentaion first commit](https://github.com/census-instrumentation/opencensus-go/commit/304ea252d1c39e8aecc84d1bb608c806ff25bfb3)
- 2019/05 : [opencensus](https://opencensus.io/) jaeger tracing, [opentracing](https://github.com/opentracing/opentracing-go) merged to [opentelemetry spec + instrumentation](https://github.com/open-telemetry/opentelemetry-java/pull/244)

Back in 2017, many other competitors provided such feature like Azure with the service map (with azure function FAAS) to vizualise service dependencies including statistics, metrics and sampling of errors and latencies.

The aim of OpenTelemetry is to provide a specification, instrumentation, exporter, collectors (agent) and backend specification.

OpenTelemetry is a [CNCF project](https://www.cncf.io/) and claims [vendor- and tool-agnostic (backend-agnostic)](https://opentelemetry.io/docs/what-is-opentelemetry/)

It starts from the instrumentation from the observed application, exported to optional collectors which flush data to backend such [Jaeger](https://www.jaegertracing.io/), [Tempo](https://grafana.com/oss/tempo/), [Prometheus](https://prometheus.io/), [Mimir](https://grafana.com/oss/mimir/), [Loki](https://grafana.com/oss/loki/), ...

All the communication between observed application to telemetry backend uses the same protocol : [OTLP](https://github.com/open-telemetry/opentelemetry-proto/tree/main/opentelemetry/proto)

## Key concepts
Reference: https://opentelemetry.io/docs/concepts/

### Signals

### Protocol

#### OTLP

##### gRPC

### Instrumentation
#### Manual
#### Automatic
https://opentelemetry.io/docs/concepts/components/#automatic-instrumentation

### Exporters
https://opentelemetry.io/docs/concepts/components/#exporters

### Collector

Collectors can support a high number of receivers and exporters but also the main protocol OTLP.

#### Sampler
https://opentelemetry.io/docs/concepts/components/#sampler
##### Head sampling
##### Tail sampling

### Cross service Propagators
https://opentelemetry.io/docs/concepts/components/#cross-service-propagators

## Components



## Architecture

## Traces

## Metrics

## Logs