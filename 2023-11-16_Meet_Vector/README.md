# 2023-11-16 #5 Meet Vector

This week, [Vector.dev](https://vector.dev/) from DataDog will be deeply analyzed.

## History
2018 : vector is born, written in rustlang.
2021 : [DataDog acquires Vector and Timber technologies](https://www.datadoghq.com/blog/datadog-acquires-timber-technologies-vector/)

### Timberio
At the beginning, timeberio was the dev team behind vector

### DataDog

### Concept

sources > transform > sinks

### License

## Features
### When to use it ?
https://vector.dev/docs/setup/going-to-prod/architecting/#choosing-agents

Basic principle

Under the hood
https://vector.dev/docs/about/under-the-hood/

### Configuration
Organize files

### Data directory
file state management

### Agent communication
2021 : Datadog acquires vector https://www.datadoghq.com/blog/datadog-acquires-timber-technologies-vector/
2019/02: birth of vector proto: https://github.com/vectordotdev/vector/commit/ba8be7dc04ddf1da1b5dd63e6039b27bdb050b40
2018/09: vector init commit: https://github.com/vectordotdev/vector/commit/83705cb791254b331b27f7719f5adae083ac3b13

In parallel
2019/05 : opencensus(https://opencensus.io/) golang jaeger tracing, opentracing(https://github.com/opentracing/opentracing-go) > opentelemetry spec + instrumentation : https://github.com/open-telemetry/opentelemetry-java/pull/244

2017/01: opencensus instrumentaion first commit (https://github.com/census-instrumentation/opencensus-go/commit/304ea252d1c39e8aecc84d1bb608c806ff25bfb3)

2015/11: opentracing spec first commit (https://github.com/opentracing/opentracing-go/commit/eab1a36e622e49f29d348dc39bc03730ae228b72)

vector event model proto: https://github.com/vectordotdev/vector/blob/master/lib/vector-core/proto/event.proto
vector service proto (push model with EventWrapper): https://github.com/vectordotdev/vector/blob/master/proto/vector.proto
proto vector spec vs otlp proto spec

Compatibility model issues between vector <> datadog <> opentelemetry.

### Transformation
Transformation Pipeline DAG : https://vector.dev/docs/about/under-the-hood/architecture/pipeline-model/

Vector graph : https://vector.dev/docs/reference/cli/#graph

### Telemetry I/O
sources and sinks

| Protocol | Input | Output |
|-|-|-|
|datadog|||

### Vector Remapping language
#### Features

#### Rust crate
https://crates.io/crates/vrl

#### Project using VRL
https://github.com/openobserve/openobserve/blob/main/Cargo.toml#L164

### Error Handling

### TDD

### Monitoring Vector

### Log to metric

## Vector as Node exporter (host metrics)

## Conclusion

How about a true opentelemetry vector ?

### Strength

+ Safe error handling
+ Documentation
+ Resiliency
+ Data Durability (buffer : https://vector.dev/docs/about/under-the-hood/architecture/buffering-model/#disk-buffers)

### Weekness

- OTLP support / Vector <> OTLP conversion and alignment issues
- Datadog vendor locking : https://github.com/vectordotdev/vector/issues/1444#issuecomment-1704040812 + https://github.com/vectordotdev/vector/blob/master/lib/vector-core/proto/event.proto#L91