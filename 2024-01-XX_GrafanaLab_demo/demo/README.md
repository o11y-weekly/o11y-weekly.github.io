# Demo
## Disclaimer
⚠️This demo is not a grafana labs production ready demo and used as local dev hands on and demo only.

Security, scalling and so on will not be introduced and GrafanaCloud offers the best experience and a no brainer solution to start with.

## Architecture

This demo includes 2 java services (service and client) and a postgres database to use webserver and jdbc instrumentations.

[OpenTelemetry Collector Contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib) has been used as a [Gateway](https://opentelemetry.io/docs/collector/deployment/gateway/) and [Agent](https://opentelemetry.io/docs/collector/deployment/agent/)

![docker compose services](./docker-compose.png)


## Run locally

### Run the docker compose
```bash
./up.sh
```

## Run Grafana
Open dashboard: http://localhost:3000

2 folders:
- App: contains app dashboard
- OpenTelemetry Collector Contrib: Agent and Gateway monitoring

## Features

### Agent Deployment
- [Agent Configuration](./otelcontribcol/agent/)
- [Gateway Configuration](./otelcontribcol/gateway/)

Gateway Tail sampling configuration:

```yaml
processors:
  tail_sampling:
    decision_wait: 10s
    policies:
      [
          # skip traces where latencies are < 1000ms
          {
            name: latency-policy,
            type: latency,
            latency: {threshold_ms: 1000}
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
                    {
                      name: http-status-code-error-policy,
                      type: string_attribute,
                      string_attribute:
                        {
                          key: error.type,
                          values: ["[1-4].."],
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

### Logs
#### Agent
Inside the service container, supervisord is used to run the agent like a kubernetes sidecar.

- Instrumentation: manual with logback and a file log
- Collector: [OpenTelemetry Collector Contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib/)
 [Agent](https://opentelemetry.io/docs/collector/deployment/agent/) which scrape the log file and send logs to loki.

#### No Collector
Inside the client app.
- Instrumentation: [OpenTelemetry Java Agent](https://github.com/open-telemetry/opentelemetry-java-instrumentation)
- Collector: [OpenTelemetry Collector Contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib/)
 [Gateway](https://opentelemetry.io/docs/collector/deployment/gateway/) which sends otlp logs to loki

 ### Traces
 - Instrumentation: automatic with [OpenTelemetry Java Agent](https://github.com/open-telemetry/opentelemetry-java-instrumentation)
- Collector: No Collector to Gateway with tail sampling on latencies + error
