REFERENCES:
https://railsadventures.wordpress.com/2019/12/02/from-graphite-to-prometheus-things-ive-learned/

# 2023-12-14 #9 Meet Prometheus
Prometheus has been created in 2012 by [SoundCloud](https://developers.soundcloud.com/blog/prometheus-monitoring-at-soundcloud) as a "pet project" and becoming popular to "slice and dice" metrics accross multi-dimensional metrics.

Like for [Graphite](../2023-12-07_Meet_Graphite/README.md) did with [RRDtool](https://oss.oetiker.ch/rrdtool/) + [Cacti](https://www.cacti.net/), Why did SoudCloud create [Prometheus](https://prometheus.io/)? 

Why SoudCloud decided to move away from Graphite ? 

What are the new trade-offs ?

Does Prometheus can cover all [Graphite](../2023-12-07_Meet_Graphite/README.md) use cases ?

## Why Prometheus ?
Reference: 
 - https://developers.soundcloud.com/blog/prometheus-monitoring-at-soundcloud
 - 

TODO: and solve multidimensional queries without impacting the backend performance like in [Graphite](../2023-12-07_Meet_Graphite/README.md#query-language).

## What is Prometheus ?

## Quickstart

## Architecture

https://prometheus.io/docs/introduction/overview/#architecture

Service discovery, ...

## Exporters

## Storage
Cortex: https://grafana.com/blog/2020/08/20/cortex-the-scalable-prometheus-project-has-advanced-to-incubation-within-cncf/, thanos, ...

## Query Language
PromQL
https://grafana.com/blog/2021/05/13/how-to-correlate-graphite-metrics-and-loki-logs/

## AlertManager

### PushGateway
https://github.com/prometheus/pushgateway

https://prometheus.io/docs/practices/pushing/
https://prometheus.io/docs/instrumenting/pushing/

## Graphing

## Additional tools

### Grafana

### Consul

## Backends comparison
[Graphite vs VictoriaMetrics vs Prometheus vs Mimir demo from previous post](../2023-11-09_Monotonicity/demo/README.md#datapoints-visualization-comparison)

## Conclusion