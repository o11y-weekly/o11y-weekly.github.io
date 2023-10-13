# 202310XX - News and What is not an observability solution ?

### TL;DR
In terms of load, each request done by an application to observe will send at least 1 signal or 3 and more depending the verbosity of the instrumentation.

It is easy to understand that in terms of load, the observability backend can become an invisible spof because observability is rarely on the critical path of the production service map.

Observability is all about trade-offs and hard / soft limitations to ensure the health and valid usage of the solution.

Putting a QoS in the transport (OTLP), in the collector (OTLP client libraries) and on the backend part can help to avoid a SRE nightmare.

<iframe width="560" height="315" src="https://www.youtube.com/embed/rZ3ETK7-ZM8?si=DtdEOm7lkF3aaiiT" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

## What is not an observability solutions

In my experience, in hotel industry and social network I was facing multiple times with limitations from the tooling, databases, ...etc.

Each tools / databases have their own limitations because they are optimized with trade-offs which are often documented and monitored (metrics/dashboard).

Here is a list of challenges I tackled in the past which was at first glance "observability" but not:

## Logs: Indexing large payload for a long term
- Indexing and storing HTTP request and response with large body (>20MB) for a long term (> 3 years) and searcheable.
## Metrics: Audit user activity at scale
- Auditing user activity for more than 70 millions users.
## Logs: Audit OTP / SMS toll fraud 
- Log OTP SMS based authentication to investigate toll fraud

## Tracing: Trace 100% of the prod volume
- Tail processor sampling

### QoS
https://en.wikipedia.org/wiki/Quality_of_service

> OTLP example where QoS is part of the protocol (no streaming) and in the client lib (max size)

> Backend (rate limiting, ...)