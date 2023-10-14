# 2023-10-XX: What is not an observability solution ?

Observability without proper trade-offs and QoS is like using shapes the wrong way.

<iframe src="https://www.youtube.com/embed/rZ3ETK7-ZM8?si=DtdEOm7lkF3aaiiT" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

## TL;DR
In terms of load, each request done by an application to observe will send at least 1 or more signals depending the verbosity of the instrumentation.

It is easy to understand that in terms of load, the observability backend can become a spof.

It can be invisible if it is not used in the critical path but this is rarely the case since metrics / alerting and associated triggers can be impacted.

Observability is all about trade-offs and hard / soft limitations to ensure the health and valid usage of the solution AND the health of the SRE team.

Putting QoS and trade-offs in the collector (ie: OTLP protocol, agents [vector max line bytes](https://vector.dev/docs/reference/configuration/sources/file/#max_line_bytes) and on the backend part can help to avoid a SRE nightmare.

---
## What are trade-offs and QoS ?

### Trade-offs

#### In transport
The [otlp protocol](https://github.com/open-telemetry/opentelemetry-proto/tree/main/opentelemetry/proto) itself is well documented. Official client library are compiled (gRPC) with embedded trade-offs (max size for AnyValue, ...)

HTTP and structured body like HTTP/json are not really documented anywhere while defining collector configuration is very important since there is not standard client library for deserializing with a proper QoS on the transport and max size per doc or line.

#### At collector level
libraries and agent

#### At backend level
> Backend (rate limiting, fair usage policy ...)

### QoS
https://en.wikipedia.org/wiki/Quality_of_service

## Well known traps

### All you need is logs
Old and legacy application uses logs only without any metrics.

Teams building dashboards tend to compute metrics over logs which highly impact the backend.

The more the panel are added, the more the backend will suffer.

Computing logs is IO and cpu intensive and this part can be precomputed for metrics dashboards.

To do it, collectors and agent can be use to transform logs to metrics.

### Metrics and high cardinality
Do not worry, this is not anymore hard limits but you will pay a lot to support it.

### Tracing all the way down
How about instrumenting at 100% and then extract metrics and logs ?

### Out of control collector
Lack of collector monitoring (library and agents) and possible DDOS due to heavy usage and bad defaults.

Sending a large payload + default agent auto retry without proper monitoring.

---

## What is not an observability solutions

In my experience, in hotel industry and social network I was facing multiple times with limitations from the tooling, databases, ...etc and obviously observability tools.

Each tools / databases have their own limitations because they are optimized with trade-offs which are often documented and monitored (metrics/dashboard).

Here is a non exhaustive list of challenges I tackled in the past which seemed to be at first glance "observability" but not:

## Logs: Indexing large payload (At-least-once delivery) for a long term (PB of logs)
Indexing and storing HTTP request and response with large body (>20MB) for a long term (> 3 years) and searcheable for a critical business risk to cover but at low cost (<1000$ a month).

Every morning the ELK stack was red or yellow + scalability and costs issues.

### Solutions

> Blob storage + Efficient Parser + Index compressed data + streaming + Fan Out/Fan In (zmq push/pull proxy and router/dealer)

References:
- [Parser combinator in real life](https://cboudereau.github.io/fsharp,/parser/combinator/2017/08/09/why-parser.html)
- [zmqdemo](https://github.com/cboudereau/zmqdemo)
- [Why we use fsharp data in prod?](https://cboudereau.github.io/fsharp/data/prod/2017/08/18/why-we-use-fsharp-data-in-prod.html)
- [How to parse a proto3 message with fparsec?](https://cboudereau.github.io/fsharp/parser/combinator/fparsec/proto3/2017/08/10/proto3-parser.html)
- [NCrafts conference](https://twitter.com/ncraftsConf/status/983306821984190464)

## Metrics: Audit user activity at scale
Auditing user activity for more than 70 millions users. The observability based solution quickly became unusable due to high cardinality problems (downtime).

### Solutions
> GCS + spark jobs

References:

- [Transfering PB of data from gcs in Rust](https://github.com/cboudereau/gcs-rsync)

## Logs: Audit OTP / SMS toll fraud 
Log OTP SMS based authentication to investigate [SMS toll fraud](https://www.twilio.com/docs/verify/preventing-toll-fraud).

### Solutions
Logs all the transaction to bigquery / gcs then analyze all phone numbers which did not provide the one time passcode then find the pattern, list frauder and then report them to the SMS provider.

> BigQuery + SQL

> BigQuery/GCS + Spark jobs

## Tracing: Trace 100% of the prod volume
Multiple users using 1 a very old feature (so bissecting does not help) impacted an entire database/cluster with a common query. The path was really hard to get with logs and metrics only, the tracing has been activated at 100% and the backend has been down for all the company.

### Solutions
> Tail processor sampling (keep errors and high latencies) + Agile / Domain Driven Design (how about removing the feature because the context might changed ?)

The value of the feature was not clear : since the code was really old and the query not efficient/designed to work at scale.

A test has been done with the business by measuring a KPI by temporary removing the feature to measure the benefits and value. It became clear that the feature was useless and removing it simply removed the spof.

In the end we were able to trace 100% of high latencies and errors in the system.
