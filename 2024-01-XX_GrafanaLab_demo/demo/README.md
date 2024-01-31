# Demo
## Disclaimer
⚠️This demo is not a grafana labs production ready demo and used as local dev hands on and demo only.

Security, scalling and so on will not be introduced and GrafanaCloud offers the best experience and a no brainer solution to start with.

<iframe src="https://www.youtube.com/embed/1kgAzLYeKGY?si=dsrZowTsOmSJ1tsj" title="OpenTelemetry LGTM demo" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

## Architecture

This demo includes 2 java services (service and client) and a postgres database to use webserver and jdbc instrumentations.

[OpenTelemetry Collector Contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib) has been used as a [Gateway](https://opentelemetry.io/docs/collector/deployment/gateway/) and [Agent](https://opentelemetry.io/docs/collector/deployment/agent/)

![docker compose services](./docker-compose.png)

## Run locally

### Run the docker compose
```bash
./up.sh
```
### Run Grafana
Open Grafana: http://localhost:3000

2 folders:
- App: contains app dashboards
- OpenTelemetry Collector Contrib: Agent and Gateway monitoring

## Grafana Dashboards

### Java

### OTEL




## Java Instrumentation

### Automatic instrumentation

### Manual instrumentation

### Metrics

Micrometer

### Logs
Logback

#### Traces
WithSpan attribute

### Java Agent

### Micrometer

### File Log

### OTEL Log exporter

### OTEL Trace exporter

## OTEL Collector

### Deployment

### Configuration

#### Agent

#### Gateway

### Traces Tail Sampling






## OpenTelemetry Collector Contrib

### Agent Deployment
- [Agent Configuration](./otelcontribcol/agent/)
- [Gateway Configuration](./otelcontribcol/gateway/)

Traces Tail sampling [configuration](./otelcontribcol/traces-collector/pipeline.traces.yml):

```yaml
processors:
  tail_sampling/latency-error:
    decision_wait: 10s
    policies:
      [
        # skip traces where latencies are < 100ms
        {
          name: latency-policy,
          type: latency,
          latency: {threshold_ms: 100}
        },
        # keep only error traces by skipping 4XX errors
        {
          name: error-policy,
          type: and,
          and:
            {
              and_sub_policy:
                [
                  {
                    name: status_code-error-policy,
                    type: status_code,
                    status_code: {status_codes: [ERROR]}
                  },
                  # exclude false positive like bad requests or not found
                  {
                    name: http-status-code-error-policy,
                    type: string_attribute,
                    string_attribute:
                      {
                        key: error.type,
                        values: [4..],
                        enabled_regex_matching: true,
                        invert_match: true,
                      },
                  },
                ]
            }
        }
      ]
```

### Metrics
- Instrumentation: micrometer with otlp registry
- Collector: No Collector

Java [maven dependency](./client/pom.xml):
```xml
<dependency>
    <groupId>io.micrometer</groupId>
    <artifactId>micrometer-registry-otlp</artifactId>
</dependency>
```

Java [micrometer-otlp configuration](./client/src/main/resources/application.yml)
```yaml
management:
  otlp:      
    metrics:
      export:
        enabled: true
        step: 10s
        url: http://mimir:9009/otlp/v1/metrics
        # url: https://prometheus-prod-24-prod-eu-west-2.grafana.net/otlp/v1/metrics
        # headers:
        #   Authorization: Basic ####
```

### Logs
#### Agent mode
Inside the service container, supervisord is used to run the agent like a kubernetes sidecar.

- Instrumentation: manual with logback and a file log
- Collector: [OpenTelemetry Collector Contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib/)
 [Agent](https://opentelemetry.io/docs/collector/deployment/agent/) which scrape the log file and send logs to loki.

OpenTelemetry Collector Agent [configuration](./otelcontribcol/agent/pipeline.app.yaml)
```yaml
receivers:
  filelog/app:
    include: [ /app/log/*.log ]
    storage: file_storage/app
    multiline:
      line_start_pattern: timestamp=
    resource:
      service.name: ${env:SERVICE_NAME}
      service.namespace: ${env:SERVICE_NAMESPACE}
      host.name: ${env:HOSTNAME}
      deployment.environment: ${env:DEPLOYMENT_ENVIRONMENT}

processors:
  batch/app:
  resource/app/loki:
    attributes:
      - action: insert
        key: loki.resource.labels
        value: service.name, service.namespace, service.version, host.name, deployment.environment, service.instance.id
      - action: insert
        key: loki.format
        value: raw

service:
  pipelines:
    logs/app:
      receivers: [filelog/app]
      processors: [batch/app, resource/app/loki]
      exporters: [loki]
```

#### No Collector mode
Inside the client app.
- Instrumentation: [OpenTelemetry Java Agent](https://github.com/open-telemetry/opentelemetry-java-instrumentation)
- Collector: [OpenTelemetry Collector Contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib/)
 [Gateway](https://opentelemetry.io/docs/collector/deployment/gateway/) which sends otlp logs to loki

Gateway [configuration](./otelcontribcol/gateway/pipeline.logs.yml) to collect logs.

```yaml
processors:
  resource/loki:
    attributes:
      - action: upsert
        key: service.instance.id # loki does not accept host.name (https://github.com/grafana/loki/issues/11786)
        from_attribute: host.name

service:
  pipelines:
    logs/gateway:
      receivers: [otlp/gateway]
      processors: [resource/loki]
      exporters: [otlphttp/gateway/loki]
```

 ### Traces
 - Instrumentation: automatic with [OpenTelemetry Java Agent](https://github.com/open-telemetry/opentelemetry-java-instrumentation)
- Collector: No Collector to Gateway with tail sampling on latencies + error

Java Agent supervisord [command line](./client/supervisor.d/app.ini)
```ini
[program:app]
directory=/app
command=java
    -javaagent:/app/opentelemetry-javaagent.jar
    -Dservice.name=%(ENV_SERVICE_NAME)s
    -Dservice.namespace=%(ENV_SERVICE_NAMESPACE)s
    -Dhost.name=%(host_node_name)s
    -Ddeployment.environment=%(ENV_DEPLOYMENT_ENVIRONMENT)s
    -Dotel.resource.attributes=service.name=%(ENV_SERVICE_NAME)s,service.namespace=%(ENV_SERVICE_NAMESPACE)s,deployment.environment=%(ENV_DEPLOYMENT_ENVIRONMENT)s,host.name=%(host_node_name)s
    -jar /app/main.jar 
    --spring.application.name=%(ENV_SERVICE_NAME)s 
autorestart=false
startretries=0
stdout_logfile=/dev/fd/1
stdout_logfile_maxbytes=0
stderr_logfile=/dev/fd/2
stderr_logfile_maxbytes=0
```