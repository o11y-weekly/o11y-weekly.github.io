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
References: 
- https://graphite.readthedocs.io/en/latest/faq.html#what-is-graphite
- https://graphite.readthedocs.io/en/latest/overview.html#about-the-project

Usually, TSDB backends are used for non functional requirements like request per second, ...

[According to the use case](https://graphite.readthedocs.io/en/latest/faq.html#who-should-use-graphite), Graphite can also be suitable for measuring business values.

> " For example, Graphite would be good at graphing stock prices because they are numbers that change over time"

[Prometheus comparison](https://prometheus.io/docs/introduction/comparison/#summary) highlight such use case

> "Prometheus offers a richer data model and query language, in addition to being easier to run and integrate into your environment. If you want a clustered solution that can hold historical data long term, Graphite may be a better choice."

The [monotonicity and temporality post](../2023-11-09_Monotonicity/README.md#cumulative-vs-delta) illustrates the fact and trade-offs.

Graphite is not a monolith and multiple components compose graphite such as [carbon](https://graphite.readthedocs.io/en/latest/carbon-daemons.html)

## Architecture

## Protocol

## Functions

## Tags
Tags are the equivalent of prometheus labels or simply labels in [OpenTelemetry](../2023-11-30_What_is_OpenTelemetry/README.md) terminology
Reference: https://graphite.readthedocs.io/en/latest/tags.html

## Telemetry temporality

delta mode.

[demo](../2023-11-09_Monotonicity/demo/README.md)

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

### When to use it ?
[Long lived counters metrics](../2023-11-09_Monotonicity/demo/README.md#long-lived-cumulative-counter) with few labels.

## Conclusion
