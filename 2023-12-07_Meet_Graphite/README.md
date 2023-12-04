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

## Protocol

## Telemetry temporality

delta mode.

[demo](../2023-11-09_Monotonicity/demo/README.md)

## Architecture

## Graphite and StatsD

## Scaling Graphite

## Compatible frontend

### Grafana
Graphite datasource

A dedicated post will be created later for Grafana

### Datadog
https://www.datadoghq.com/blog/dogstatsd-mapper/

A dedicated post will be created later for Datadog

## Compatible backends

### Mimir

### VictoriaMetrics