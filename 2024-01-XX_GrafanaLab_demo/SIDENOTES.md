## logs
references: 
- https://opentelemetry.io/docs/specs/otel/logs/#direct-to-collector
- https://github.com/open-telemetry/opentelemetry-java-examples/tree/main/spring-native
- https://opentelemetry.io/docs/languages/java/automatic/spring-boot/
- https://github.com/grafana/grafana-opentelemetry-starter/blob/main/build.gradle
- https://github.com/spring-projects/spring-boot/issues/37278
- https://github.com/open-telemetry/opentelemetry-java/blob/main/sdk-extensions/autoconfigure/README.md


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
