# OpenTelemetry Looks Good To Me

LGTM are a GrafanaLabs joke to quickly name Loki, Grafana, Tempo and Mimir which have been used in this [demo](./demo/README.md).

[OpenTelemetry](../2023-11-30_What_is_OpenTelemetry/README.md) become standard and really useful to solves common problems in observability.

[Dedicated post for the docker compose GrafanaLabs LGTM for java with OpenTelemetry demo](./demo/README.md)

## Naming conventions
References: https://opentelemetry.io/docs/concepts/semantic-conventions/

Without naming and value conventions, correlating signal can become quickly a nightmare, to solve that issue, [OpenTelemetry Semantic Conventions](https://opentelemetry.io/docs/concepts/semantic-conventions/) concept is really good to start and adopt.

In the following demo, [resources](https://opentelemetry.io/docs/languages/js/resources/#:~:text=A%20resource%20represents%20the%20entity,be%20included%20in%20the%20resource.) have been defined to have such attributes for all signals to query/correlated telemetry for those dimensions.

| Resource Attribute | Value |
|-|-|
|service.name| the application name |
|service.namespace| namespace to group multiple service [Bounded context](https://martinfowler.com/bliki/BoundedContext.html) |
|service.version| version of the service |
|service.instance.id | hostname |
|deployment.environment | the environment where the application runs (dev,test,prod, ...) |

As an example, by clicking or errors, it can be easy to switch to get corresponding traces:

![errors to traces](./errors_to_traces.png)
![traces](./traces.png)
![trace](./trace.png)

## Traces Tail Sampling
This demo uses tail sampling introduced during the last [Tail sampling post](../2023-11-30_What_is_OpenTelemetry/README.md#tail-sampling).

By default, head sampling is used to decided the percentage of the traffic that should be traced.
Parent based head sampling is commonly used to avoid truncated spans but can become inefficient to optimize p99 requests. 

While starting with head sampling can be done easily with good results, tracking p99 optimization can be hard since it really depends of the ratio and the probability to get the full trace. When the ratio is close to 100%, it means that almost all the traces end to the traces backend and come at cost.

No sampling is just a waste of resource since only errors and latencies are important.

Tail sampling has been setup with [opentelemetry collector contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib) in this [demo](./demo/otelcontribcol/pipeline.gateway.yml).

Again, when tracing error, it become important to avoid tracing 4XX errors when it is not useful or important.

## Standard dashboards

Along this demo, 2 dashboards has been built:


## logs
references: 
- https://opentelemetry.io/docs/specs/otel/logs/#direct-to-collector
- https://github.com/open-telemetry/opentelemetry-java-examples/tree/main/spring-native
- https://opentelemetry.io/docs/languages/java/automatic/spring-boot/
- https://github.com/grafana/grafana-opentelemetry-starter/blob/main/build.gradle
- https://github.com/spring-projects/spring-boot/issues/37278
- https://github.com/open-telemetry/opentelemetry-java/blob/main/sdk-extensions/autoconfigure/README.md

## OTEL > LOKI labels mapping
References: https://github.com/grafana/loki/blob/main/docs/sources/send-data/otel/_index.md

TODO PR to add service.version: https://github.com/grafana/loki/blob/de251c3fc2cbd3e9de8d19bd986680c2b99c88bc/pkg/loghttp/push/otlp.go#L31


## java agent 2
https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/tag/v2.0.0

# Correlation and naming conventions are important

# otel resource conventions
https://opentelemetry.io/docs/specs/semconv/resource/

# metric naming conventions
https://opentelemetry.io/docs/specs/semconv/general/metrics/


## Maven build info to get version
https://github.com/open-telemetry/opentelemetry-java-instrumentation/pull/9480

## live demo
<iframe width="1920" height="1080" src="https://www.youtube.com/embed/Hrq4-HouO-s?si=vDB68ywkS0UddXai" title="GrafanaCon CFP demo" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>