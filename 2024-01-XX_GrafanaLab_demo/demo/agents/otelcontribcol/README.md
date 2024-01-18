# otelcontribcol

## datamodel
https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/metrics/data-model.md#gauge

## Getting started

https://opentelemetry.io/docs/collector/getting-started/

## Pipeline
https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/pkg/stanza/docs/operators

## Parsing
https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/processor/logstransformprocessor

https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/pkg/stanza/docs/types/parsers.md#complex-parsers

## Log fingerprint/checkpoints

storage : https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/extension/storage/filestorage

## Deployment

Baby step : No Collector

https://opentelemetry.io/docs/collector/deployment/

## Log to metric

:warning: only counter is implemented at the time of writing (very limited usage)

https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/connector/countconnector

## Troubleshooting
https://github.com/open-telemetry/opentelemetry-collector/blob/main/docs/troubleshooting.md
https://grafana.com/docs/opentelemetry/collector/troubleshooting/?src=---------3------------------&pg=pricing&plcmt=pricing-calculator-pro&cta=start-free-trial

## Transformation
### Language
https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/pkg/ottl/README.md

## Drop pattern
https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/pkg/stanza/docs/types/on_error.md

https://opentelemetry.io/docs/specs/otel/protocol/otlp/#failures

Drop on parsing failure but filter error and action drop to support dead letter queue in loki for instance

## Monitoring
https://github.com/open-telemetry/opentelemetry-collector/blob/main/docs/monitoring.md

https://github.com/open-telemetry/opentelemetry-collector/issues/5300

https://github.com/open-telemetry/opentelemetry-collector/blob/80d704deb46021176c3fc408f63496773388f3b1/service/telemetry/config.go