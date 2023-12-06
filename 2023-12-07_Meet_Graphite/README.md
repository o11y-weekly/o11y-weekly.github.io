# 2023-11-30 #8 Meet Graphite

Graphite has been created at Orbitz (2006), a hotel industry actor to monitor and support their growth.

Other Time Series Database [TSDB](https://en.wikipedia.org/wiki/Time_series_database) was already there before like [RRDtool](https://en.wikipedia.org/wiki/RRDtool).

Why Orbitz decided to not use [RRDtool](https://oss.oetiker.ch/rrdtool/) + [Cacti](https://www.cacti.net/) and created Graphite ?

Is Graphite still worth over other new and existing solutions ?

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

[According to the bellow use case](https://graphite.readthedocs.io/en/latest/faq.html#who-should-use-graphite), Graphite can also be suitable for measuring business values:

> "For example, Graphite would be good at graphing stock prices because they are numbers that change over time."

[Prometheus comparison](https://prometheus.io/docs/introduction/comparison/#summary) highlights such use case

> "Prometheus offers a richer data model and query language, in addition to being easier to run and integrate into your environment. If you want a clustered solution that can hold historical data long term, Graphite may be a better choice."

The [monotonicity and temporality post](../2023-11-09_Monotonicity/README.md#cumulative-vs-delta) illustrates the fact and trade-offs.

Graphite is not a monolith and multiple components compose graphite such as [carbon](https://graphite.readthedocs.io/en/latest/carbon-daemons.html)

## Quickstart
Reference: https://graphite.readthedocs.io/en/latest/install.html#docker

Using [graphite with docker](https://graphite.readthedocs.io/en/latest/install.html#docker) is the easiest way to test graphite quickly.

The docker image is not production ready though and many components are installed by default to make it easy to use for development but not for production.

A demo with other backends is available on a [previous post demo](../2023-11-09_Monotonicity/demo/README.md#context)

## Architecture and Scalability

Graphite has been forked and updated to support scalability at different scope over time since the project has a long history since 2006.

Projects: 
- [Graphite](https://github.com/graphite-project)
- [Go Graphite](https://github.com/go-graphite)

An excellent (old) post from Teads mentioned how to scale graphite: https://medium.com/teads-engineering/scaling-graphite-in-a-cloud-environment-6a92fb495e5

Graphite can be viewed as a backend or as a protocol and other backends are compatible with it, like prometheus, mimir, victoriametrics but with different aggregation temporality which can conflict with the main feature of graphite ([long lived cumulative counter](../2023-11-09_Monotonicity/demo/README.md#long-lived-cumulative-counter)).

/!\ All backends are not fully compliant with [long lived counters](../2023-11-09_Monotonicity/demo/README.md#long-lived-cumulative-counter) and if this feature matter, it is important to scale the data storage first or any other graphite components like the [Go Graphite](https://github.com/go-graphite) does.

### whisper
Reference: https://github.com/graphite-project/whisper

Differences with RRD: https://graphite.readthedocs.io/en/latest/whisper.html#differences-between-whisper-and-rrd

Whisper is the default TSDB with graphite. Graphite can support [much more TSDB](https://graphite.readthedocs.io/en/1.1.8/tools.html#storage-backend-alternates
) with different trade-offs (clickhouse, InfluxDB, ...).

### carbon
References: 
- https://github.com/graphite-project/carbon
- https://graphite.readthedocs.io/en/stable/carbon-daemons.html

Carbon is the write path of the metrics signal. It serves different purpose like: 

- Replicate and shard writes to backend (ie: whisper)
- Rewrite metrics
- Allow or Block metrics
- Aggregate metrics

### graphite-web
Reference: https://github.com/graphite-project/graphite-web

As opposed to carbon, graphite-web is responsible for the metric read path. This component serves the api and graph visualization.

Usually, only the [api](https://graphite-api.readthedocs.io/en/latest/) part of graphite-web is used in conjuction with a frontend like [grafana](https://grafana.com/grafana/).

## Protocol
Reference: https://graphite.readthedocs.io/en/latest/feeding-carbon.html

Carbon supports many protocols but the most used is the straightforward plain text protocol.

### Plain Text
`<metric path> <metric value> <metric timestamp>`

```bash
PORT=2003
SERVER=graphite.your.org
echo "local.random.diceroll 4 `date +%s`" | nc ${SERVER} ${PORT}
```
### Labels
Reference: https://graphite.readthedocs.io/en/latest/tags.html

Depending the backend configuration, the `<metric path>` can contain tags (aka labels).

`my.series;tag1=value1;tag2=value2`

## StatsD
Reference: https://www.etsy.com/codeascraft/measure-anything-measure-everything/

[StatsD](https://github.com/statsd/statsd) has been [created by Etsy](https://www.etsy.com/codeascraft/measure-anything-measure-everything/) to send metrics without performance overhead or simply impacting SLA when the metrics backend is dead. By simply using UDP to send metrics to StatsD, the observed application is not responsible anymore to manage state and is decoupled from the metrics backend which is good if SLAs are different. StatsD also reduces the rate and send data at a given resolution (ie: 10s).

The protocol is not the same as Graphite but simpler and still plain text: `<metricname>:<value>|<type>`

```bash
echo "foo:1|c" | nc -u -w0 127.0.0.1 8125
```

A demo is available from this previous post: [graphite + statsd vs other backends](../2023-11-09_Monotonicity/demo/README.md#context) with [this statsd udp configuration](../2023-11-09_Monotonicity/demo/graphite/statsd/udp.js)

## Archiving old data
Reference: https://graphite.readthedocs.io/en/latest/whisper.html#archives-retention-and-precision

Optimizing space over the time is crucial. Data can simply be deleted or compressed. Compression can be lossless or lossy and depending the use case, supporting both can be a good idea.

It is possible to setup lossy compression by increasing the resolution period datapoint. A datapoint can be at a resolution of 10s for the last 3 months then at 1 minute to reduce space by 6 (60s / 10s).

## Telemetry temporality

As mentioned in [OpenTelemetry metrics temporality](../2023-11-30_What_is_OpenTelemetry/README.md#metrics) and the [Monotonicity demo](../2023-11-09_Monotonicity/demo/README.md#long-lived-cumulative-counter) graphite is a delta metrics temporality backend which support [long lived cumulative counter](../2023-11-09_Monotonicity/demo/README.md#long-lived-cumulative-counter).

## Additional tools

### Grafana
Reference: https://grafana.com/docs/grafana/latest/datasources/graphite/

Grafana comes from the concatenation of 2 words: `Graphite` and `Kibana` to make Graphite visualization as smooth as possible.

The main difference between Grafana and other competitors is a cached datasource support without involving a full synchronization which impacts resources and costs.

Grafana offers the best integration for graphite since it has been created for.

According to the OTLP and prometheus Grafana intregration, Grafana metrics backend like Mimir supports only cumulative metrics while graphite is a true delta metrics. A dedicated post is comparing [the pros and cons of delta and cumulative temporality](../2023-11-09_Monotonicity/README.md).

A dedicated post will be created later for Grafana.

### Datadog
Reference: https://www.datadoghq.com/blog/dogstatsd-mapper/

Datadog has a centralized model where all the data should be stored inside its database which is a bit different from Grafana since you can choose via a [collector](../2023-11-30_What_is_OpenTelemetry/README.md#collector) to sync or fetch and cache data.

Datadog is a drop-in solution of graphite but seems supporting delta temporality.

A dedicated post will be created later for Datadog.

## Backends comparison
[Graphite vs VictoriaMetrics vs Prometheus vs Mimir demo from previous post](../2023-11-09_Monotonicity/demo/README.md)

## Conclusion
Graphite powered etsy, clickhouse metrics and has changed significantly to support label and scalability. In the meantime, prometheus won the battle for the observability and rates monitoring while delta modes and other usecases are not fully covered by those alternatives.

As mentioned by the prometheus team, graphite is best at supporting [long lived cumulative counters](../2023-11-09_Monotonicity/demo/README.md#long-lived-cumulative-counter) with few labels.

As soon as scalability becomes important for metrics, labels and pure observability, other solutions fit could be considered.