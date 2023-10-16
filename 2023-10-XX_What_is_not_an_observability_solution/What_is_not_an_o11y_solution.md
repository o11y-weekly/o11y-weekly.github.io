# 2023-10-XX: What is not an observability solution ?

Observability without proper trade-offs and QoS is like using shapes the wrong way.

<iframe src="https://www.youtube.com/embed/rZ3ETK7-ZM8?si=DtdEOm7lkF3aaiiT" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

## TL;DR

Quote: 
>"Simplicity is prerequisite for reliability" Edsger Dijkstra.


Reference: [QoS in cloud computing](https://jisajournal.springeropen.com/articles/10.1186/s13174-014-0011-3)

> "Quality-of-Service (QoS) management, [..] is the problem of allocating resources to the application to guarantee a service level along dimensions such as performance, availability and reliability."

In terms of load, each request done by an application to observe will send at least 1 or more signals depending the verbosity of the instrumentation.

It is easy to understand that in terms of load, the observability backend can become a spof.

It can be invisible when it is not used in the critical path but this is rarely the case since metrics / alerting and associated triggers can be impacted.

Observability is all about trade-offs and hard / soft limitations to ensure the health and valid usage of the solution AND the health of the SRE team.

Putting QoS and trade-offs in the collector (ie: OTLP protocol, agents [vector max line bytes](https://vector.dev/docs/reference/configuration/sources/file/#max_line_bytes)) and on the backend part can help to avoid a SRE nightmare.

A decentralized responsibility model really helps to find acceptable trade-offs quickly since the SRE are the developers or in the same team. 
As opposed to decentralized model, finding trade-offs in a Federeated or Centralized model can be harder since it is easy to just push the telemetry data without caring of the observability platform health and impact then become the worst SRE's nightmare.

If the observability platform complexity is higher than the business you are monitoring, then it should raise that usage and use cases aren't really defined and might be out of control.

This is why trade-offs and QoS should be defined early.

---
## What are trade-offs and QoS ?

### Trade-offs

#### In transport
The [otlp protocol](https://github.com/open-telemetry/opentelemetry-proto/tree/main/opentelemetry/proto) itself is well documented. Official client libraries are compiled (gRPC) with embedded trade-offs (max size for AnyValue, ...)

HTTP and structured body like HTTP/json are not really documented anywhere while defining collector configuration is very important since there is no standard client libraries for deserializing with a proper QoS on the transport and max size per doc or line.

#### At collector level
Agent and libraries used should be properly configured like retrying, rates, batch mode, dead letter queue and good defaults.

The collector part will push in a decentralized way all the telemetry data. Without [aggregator](https://vector.dev/docs/setup/going-to-prod/arch/aggregator/), [gateway](https://opentelemetry.io/docs/collector/deployment/gateway/) or backend protections (rate limiting, max payload size) those components are critical since it can be the root cause of performance and "unfair usage" issue.

Tons of legacy started with logs only where dashboards and alerting use metrics for. Connecting logs to alerting should be splitted to avoid high usage for a small need. Converting logs to metrics can help to reduce IO and CPU usage and also rate to simply reduce the backend pressure and complexity.

#### At backend level
The lack of backend protections can lead to undesirable downtime for the whole platform. It is important in Federated and Centralized models that the backend should be well configured for the sake of the platform health, availibility, fairness.

"One team abusing of logs or any telemetry should not impact others or the entire platform".

Monitoring the observability platform should be in place day 1.

The [USE method](https://www.brendangregg.com/usemethod.html) oriented dashboard can really help to find out the problem.

### QoS in observability
Reference : 
- [QoS in Computer Network](https://en.wikipedia.org/wiki/Quality_of_service)
- [QoS in Cloud Computing](https://jisajournal.springeropen.com/articles/10.1186/s13174-014-0011-3)

> "Quality-of-Service (QoS) management, [..] is the problem of allocating resources to the application to guarantee a service level along dimensions such as performance, availability and reliability."

## Well known traps

### All you need is logs
Old and legacy applications use logs only without any metrics.

Teams building dashboards tend to visualize metrics over logs with high impact on the backend.

The more the panel are added, the more the backend will suffer.

Computing logs is IO and CPU intensive and this part can be precomputed for metrics dashboards.

To do it, collectors and agent can be use to transform logs to metrics.

Converting logs to metrics reduces CPU, IO usage but also rate (DPM : datapoint per minute). You can easily reduce 100K DPM to 5 DPM (5 metrics like max, min, p99, median, average per minute).

### Metrics and high cardinality
Do not worry, this is not anymore a hard limit but you will pay a lot to support it.

This was the nightmare of SRE using prometheus. Converting logs to metrics without caring about the number of timeseries can become a nightmare.

You do not want to have a counter per client user but for the whole app. In case of errors, it should be easy to correlate and to look for logs and so on.

As usual, pushing telemetry without wondering of how to view it can quickly become ineffective.

### Tracing all the way down
How about instrumenting at 100% and then extract metrics and logs ?

Tracing is the most verbose mode in the observability 3 pillars (lets talk later about a more verbose one : profiles and flamegraph).

Tracing instrumentations can also impact the observed applications (tracing 100% of the production) and collecting metrics by using tracing is an inefficient and can be an ineffective way to get usefull metrics.

Instead of relying to one backend to get metrics, the SRE team should maintain traces and metrics with the highest SLAs where tracing can offer lower SLA than metrics as a good trade-offs.

### Out of control collector
Lack of collector monitoring (library and agents) and possible DDOS due to heavy usage and bad defaults.

Sending a large payload + bad default agent configuration with auto retry without proper monitoring.

---

## What is not an observability solutions ?

In my experience, in hotel industry and social network I was facing multiple times with limitations from the tooling, databases, ...etc and obviously observability tools.

Each tools / databases have their own limitations because they are optimized with trade-offs which are often documented and monitored (metrics/dashboard).

Here is a non exhaustive list of challenges I tackled in the past which seemed to be at first glance "observability" but not:

## Logs: Indexing large payload (At-least-once delivery) for a long term (PB of logs)
Indexing and storing HTTP request and response with large body (>20MB) for a long term (> 3 years) and searcheable for a critical business risk to cover but at low cost (<1000$ a month).

Every morning the ELK stack was red or yellow + scalability and costs issues.

### Solutions

> Message-oriented Middleware + Blob storage + Efficient Parser + Index compressed data + streaming + Fan Out/Fan In (zmq push/pull proxy and router/dealer)

References:
- Indexing: [Parser combinator in real life](https://cboudereau.github.io/fsharp,/parser/combinator/2017/08/09/why-parser.html)
- Broker : [zmqdemo](https://github.com/cboudereau/zmqdemo)
- Deep dive: [Why we use fsharp data in prod?](https://cboudereau.github.io/fsharp/data/prod/2017/08/18/why-we-use-fsharp-data-in-prod.html)
- Deep dive: [How to parse a proto3 message with fparsec?](https://cboudereau.github.io/fsharp/parser/combinator/fparsec/proto3/2017/08/10/proto3-parser.html)
- Hands-on: [NCrafts conference](https://twitter.com/ncraftsConf/status/983306821984190464)

## Metrics: Audit user activity at scale
Auditing user activity for more than 70 millions users. The observability based solution quickly became unusable due to high cardinality problems (downtime).

### Solutions
> GCS + spark jobs

References:

- Deep dive: [Transfering PB of data from gcs in Rust](https://github.com/cboudereau/gcs-rsync)

## Logs: Audit OTP / SMS toll fraud 
Log OTP SMS based authentication to investigate [SMS toll fraud](https://www.twilio.com/docs/verify/preventing-toll-fraud).

### Solutions
Log all the transactions to bigquery / gcs then analyze all phone numbers without any passcode response, find the pattern, list frauders and report them to the SMS provider.

> BigQuery + SQL + GCS + Spark jobs

## Tracing: Trace 100% of the prod volume
Multiple users using 1 a very old filtering feature (so bissecting does not help) impacted an entire database/cluster with a common query. The path was really hard to get with logs and metrics only, the tracing has been activated at 100% and the backend has been down for all the company.

### Solutions
> Tail processor sampling (keep errors and high latencies) + Agile / Domain Driven Design (how about removing the filtering feature because the context has changed ?)

The value of the filtering feature was not clear : since the code was really old and the query not efficient/designed to work at scale.

A test has been made with the business by measuring an existing KPI by temporary removing the filtering feature to measure the benefits and value. It became clear that the filtering feature was useless and removing it simply removed the spof.

In the end we were able to trace 100% of high latencies and errors in the system.

## What next ?
Next week, I will introduce trade-offs and QoS examples and a matrix to quickly identify non observability or unreasonable solutions.