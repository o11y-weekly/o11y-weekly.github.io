# 2023-11-30 #8 Meet Graphite

Graphite has been created at Orbitz (2006), a hotel industry actor to monitor and support their growth.

Other Time Series Database [TSDB](https://en.wikipedia.org/wiki/Time_series_database) was already there before like [RRDtool](https://en.wikipedia.org/wiki/RRDtool).

Why Orbitz decided to not use [RRDtool](https://oss.oetiker.ch/rrdtool/) + [Cacti](https://www.cacti.net/) and created Graphite ?

## Why Graphite ?
Reference: https://graphite.readthedocs.io/en/latest/faq.html#does-graphite-use-rrdtool

A problem in RRDtool is that it does not really support temporary absence of the data (null/nil/None data) and uses zero `0` which is a good default but depending the usage it is not.

> How to calculate throughput ? If the latency down to `0`, then the throughput is infinite ?

According to the `null` latency value use case, using `0` is not a good trade-offs at all and this is the first reason why Orbitz team decided to create Graphite ([before 2006](https://graphite.readthedocs.io/en/latest/faq.html#does-graphite-use-rrdtool)).

## What is Graphite ?
Reference: https://graphite.readthedocs.io/en/latest/faq.html#what-is-graphite

## Protocol

## Telemetry temporality

delta mode.

[demo](../2023-11-09_Monotonicity/demo/README.md)

## Architecture

## Graphite and StatsD

## Scaling Graphite

## Compatible frontend

### Grafana
Reference: https://grafana.com/docs/grafana/latest/datasources/graphite/

Grafana comes from the concatenation of 2 words: `Graphite` and `Kibana` to make Graphite visualization as smooth as possible.

The main difference between Grafana and other competitors is a support of datasource and caching without involving a full synchronization which impact resources and costs.

Grafana offers the best integration for graphite since it has been created for.

According to the OTLP and prometheus Grafana intregration, Grafana metrics backend like Mimir supports only cumulative metrics while graphite is a true delta metrics. A dedicated post is comparing [the pros and cons of delta and cumulative temporality](../2023-11-09_Monotonicity/README.md).

A dedicated post will be created later for Grafana

### Datadog
Reference: https://www.datadoghq.com/blog/dogstatsd-mapper/

Datadog has model where all the data should be stored inside Datadog which is a bit different from Grafana since you can choose via a [collector](../2023-11-30_What_is_OpenTelemetry/README.md#collector) to sync or fetch and cache data.

A dedicated post will be created later for Datadog

## Compatible backends

### Mimir

### VictoriaMetrics

## Demo
- [Graphite vs VictoriaMetrics vs Prometheus vs Mimir demo](../2023-11-09_Monotonicity/demo/README.md)
