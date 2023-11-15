# 2023-11-16 #5 Meet Vector

This week, [Vector.dev](https://vector.dev/) from DataDog will be deeply analyzed.

## History
2018 : [vector is born](https://github.com/vectordotdev/vector/commit/83705cb791254b331b27f7719f5adae083ac3b13) and written in rust.
2021 : [DataDog acquires Vector and Timber technologies](https://www.datadoghq.com/blog/datadog-acquires-timber-technologies-vector/)

Since vector has been acquired by DataDog, it turns out that vector is more focused on gateway than agent to be used as a pipeline ([Directed Acyclic Graph: DAG](https://vector.dev/docs/about/under-the-hood/architecture/pipeline-model/))

## License
[Mozilla Public License 2.0](https://github.com/vectordotdev/vector/blob/master/LICENSE)

## When to use it ?
https://vector.dev/docs/setup/going-to-prod/architecting/#1-use-the-best-tool-for-the-job

https://vector.dev/docs/setup/going-to-prod/architecting/#choosing-agents

Basic principle

Under the hood
https://vector.dev/docs/about/under-the-hood/

## Concept

Component: source or transform or sink, it is a component of the pipeline (this notion is largely used when monitoring vector per component id)

### Sources
https://vector.dev/docs/reference/configuration/sources/

### Transforms
https://vector.dev/docs/reference/configuration/transforms/

Transformation Pipeline DAG : https://vector.dev/docs/about/under-the-hood/architecture/pipeline-model/

#### Pipeline Graph
Vector graph : https://vector.dev/docs/reference/cli/#graph

#### Converting log to metric
Here is the vector log_2_metric pipeline graph for the next vector demo:
![vector graph demo](./vector-graph.svg)

#### Vector Remapping language

##### Rust crate
https://crates.io/crates/vrl

##### Project using VRL
https://github.com/openobserve/openobserve/blob/main/Cargo.toml#L164

### Sinks
https://vector.dev/docs/reference/configuration/sinks/

### Configuration
Organize files

### Data directory
file state management

### I/O Telemetry Compatibility
sources (input) and sinks (output)

| Protocol | Input | Output |
|-|-|-|
|OTLP metric|||
|OTLP log|||
|OTLP trace|||
|vector|X|X|

### Error Handling

### TDD

### Monitoring Vector

## Vector as Node exporter (host metrics)

### Agent communication
Agent communication is an important topic to understand when talking about observability agent.
Vector uses its own gRPC protocol between agents/gateways. 
- [vector event model proto](https://github.com/vectordotdev/vector/blob/master/lib/vector-core/proto/event.proto)
- [vector service proto (push model with EventWrapper)](https://github.com/vectordotdev/vector/blob/master/proto/vector.proto)
proto vector spec vs otlp proto spec

Instead of using an external standard, vector has a internal protocol which can create incompatibility and [OTLP integration issues](https://github.com/vectordotdev/vector/issues/1444#issuecomment-1704040812).

It seems that log and metrics are really well integrated but tracing has a limited support and only available for datadog since the only trace sink available is the [datadog traces sink](https://vector.dev/docs/reference/configuration/sinks/datadog_traces/).

Again, 3 protocols has to be synced (vector, datadog and OTLP) which is a complex problem when there is alignment issues.

The [vector protocol contains specific DataDog metadata](https://github.com/vectordotdev/vector/blob/master/lib/vector-core/proto/event.proto#L91) which can be strange from the standard point of view.

![vector protocol contains specific features](./vector-protocol-datadog.png)

The context history can help to understand the trade-off. It seems that the vector team did not use or see the benefits of using opentracing or opentelemetry to [serialize telemetry to disk](https://github.com/vectordotdev/vector/pull/81).

- 2021 : [Datadog acquires vector](https://www.datadoghq.com/blog/datadog-acquires-timber-technologies-vector/)
- 2019/02: birth of [vector protocol](https://github.com/vectordotdev/vector/pull/81) which is the main communication protocol between agents and gateway at the time of writing.
- 2019/05 : [opencensus](https://opencensus.io/) jaeger tracing, [opentracing](https://github.com/opentracing/opentracing-go) merged to [opentelemetry spec + instrumentation](https://github.com/open-telemetry/opentelemetry-java/pull/244)
- 2018/09: [vector initial commit](https://github.com/vectordotdev/vector/commit/83705cb791254b331b27f7719f5adae083ac3b13)
- 2017/01: [opencensus instrumentaion first commit](https://github.com/census-instrumentation/opencensus-go/commit/304ea252d1c39e8aecc84d1bb608c806ff25bfb3)
- 2015/11: [opentracing spec first commit](https://github.com/opentracing/opentracing-go/commit/eab1a36e622e49f29d348dc39bc03730ae228b72)

Internally, DataDog has built a UI over vector + aggregated telemetry over vector graphs

## Conclusion

Vector is good at log and metric pipeline transformation and really well integrated with DataDog.

Its compatibility outside DataDog is limited to log and metric 

How about a true opentelemetry based vector ?

### Strength

+ Safe error handling
+ Documentation
+ Resiliency
+ Data Durability see [buffer](https://vector.dev/docs/about/under-the-hood/architecture/buffering-model/#disk-buffers)

### Weakness

- OTLP support / Vector <> OTLP conversion and alignment issues
- Datadog vendor locking : https://github.com/vectordotdev/vector/issues/1444#issuecomment-1704040812 + https://github.com/vectordotdev/vector/blob/master/lib/vector-core/proto/event.proto#L91