# 2023-11-30 #7 What is OpenTelemetry ?

## History
source : https://devops.com/a-history-of-distributed-tracing/#:~:text=The%20History%20of%20Distributed%20Tracing,open%20source%20distributed%20tracing%20project

- 2010: [Dapper](https://research.google/pubs/pub36356/)
- 2012: [Zipkin](https://www.uber.com/en-FR/blog/distributed-tracing/)
- 2015: [Jaeger](https://www.jaegertracing.io/)
- 2015/11: [opentracing spec first commit](https://github.com/opentracing/opentracing-go/commit/eab1a36e622e49f29d348dc39bc03730ae228b72)
- 2016 [Towards Turnkey Distributed Tracing post](https://medium.com/opentracing/towards-turnkey-distributed-tracing-5f4297d1736)
- 2017/01: [opencensus instrumentaion first commit](https://github.com/census-instrumentation/opencensus-go/commit/304ea252d1c39e8aecc84d1bb608c806ff25bfb3)
- 2019/05 : [opencensus](https://opencensus.io/) jaeger tracing, [opentracing](https://github.com/opentracing/opentracing-go) merged to [opentelemetry spec + instrumentation](https://github.com/open-telemetry/opentelemetry-java/pull/244)

Back in 2017, many other competitor provided such feature like Azure with the service map (with azure function FAAS) to vizualise service dependencies including statistics, metrics and sampling of errors and latencies.

## Key concepts

### Instrumentation
#### Manual
#### Automatic
https://opentelemetry.io/docs/concepts/components/#automatic-instrumentation

### Exporters
https://opentelemetry.io/docs/concepts/components/#exporters

### Collector

### Sampler
https://opentelemetry.io/docs/concepts/components/#sampler
#### Head sampling
#### Tail sampling

### Cross service Propagators
https://opentelemetry.io/docs/concepts/components/#cross-service-propagators

## Components



## Architecture

## Traces

## Metrics

## Logs