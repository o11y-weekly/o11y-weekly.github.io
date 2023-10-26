# 2023-10-27 #3 Defining Good Observability backend Trade-offs and QoS

This week in o11y, the third post about Trade-offs and QoS management in observability backend.

The previous post introduced high level QoS and trade-offs. This post focuses on defining QoS and trade-offs for the observability backend side only.

## Trade-offs and QoS

This post is an example of what could be good trade-offs and QoS. If the purpose is to scale and integrate more teams or simply tackling a legacy system, it is always a good idea to think about data life cycle.

Starting from the 2 quotes of the last week;

Quote: 
>"Simplicity is prerequisite for reliability" Edsger Dijkstra.

Reference: [QoS in cloud computing](https://jisajournal.springeropen.com/articles/10.1186/s13174-014-0011-3)

> "Quality-of-Service (QoS) management, [..] is the problem of allocating resources to the application to guarantee a service level along dimensions such as performance, availability and reliability."

For each signal, those properties are very important to know when using SAAS products (GrafanaCloud, DataDog, Splunk and so on) because it will impact the bill in the end of month or define the required skills of the Ops and SRE team responsible to build and maintain the observability backends.

The motto is to keep it stupid simple and if the use case is too much high, it just means that it is not a __full__ observability solution.

## Non Functional Requirements (NFRs)

Using observability solutions to cover a risky business can be challenging and those Non Functional Requirements should be defined day 1 to avoid a complete SLA mess.

Is it a good thing to measure business SLAs on an observability stack which does not guarantee equal or higher SLAs ?

How about finding reasonable trade-offs instead of supporting higher SLAs than required. Is it required to support / pay a complex observability stack with higher SLAs than required ?

According to Edsger Dijkstra, "Simplicity is prerequisite for reliability" and this is why __defining those NFRs is important to reduce the complexity to support high SLA.__

Creating a POC to analyze output is a good idea by using an agent and a file output. By using opentelemetry collector contrib with matching input sources (prometheus / graphite / file log / tracing / receivers which fit with the telemetry app) and otlp file output, it is really easy __to estimate required telemetry capacity.__

- __Retention__: in GB per day. Throughput and signal size are important to define the total retention. Asking about archiving or deleting signals after the retention period should be defined. In general, after the 30 days period, signals are deleted. 

- __Size__: size of the signal, it can be the number of series or just the size of the log per document/line.

- __Rate__: at which signal / datapoint per minute signals will be sent (min/max/avg/p99).

- __Risk to cover__: define the SLA of the solution (equal/less/higher than monitored app). Is the observability solution used from a support team with external client ? Is it only used by developpers or production ops / support team / care team ?

- __Precision__: Reducing the precision can help to reduce the complexity like using metrics instead of logs or traces. Does aggregating/rounding/sampling impact the expected result ? Like converting logs to metrics which can result of an approximated rates instead of a real count. How about reducing the precision over the time ?

- __Delivery__: Usually best effort in collectors and libraries. What if the log is lost at collector/transport/backend level ? At least once delivery + idempotency offer the best delivery but at which cost ?

- __Query__: 1 dimension (time range + 1 dimension) to Fulltext. Having fulltext by default is not a good idea and highly increase the complexity. 

Combining fulltext search + at least once delivery + high precision at high rates and large document for a high risk to cover is just too much high!

If thoses properties are important and the risk is high, how about investing on a dataplatform (opentelemetry collector contrib kafka exporter, bigquery, tableau, grafana+biquery...)?

Simply just replacing fulltext search by 1 dimensions reduces the complexity. 

At least once delivery does not fit with collector and instrumentation/logs libraries. In case of a failure, messages are allocated somewhere with a finite capacity and __always drop__ new messages in case of errors. 

A message-oriented middleware should be in place on the observability backend side instead of logging libraries to support such properties. As example, ZMQ ["high water mark"](https://zguide.zeromq.org/docs/chapter2/#High-Water-Marks) is a very well documented 0 broker which can support such guarantee but by __blocking__ in worst case. Combined with a queueing system, it can be really easy to support those guarantees.

While integrating more complex datasource to support corner cases, it is still possible to use observability backends or to support datasource apis like in Grafana to visualize datapoints.

[Five whys](https://en.wikipedia.org/wiki/Five_whys) 
- Why Is it required? The support team need it.
- Why the support team needs those logs ? To investigate over bookings?
- Why trade-offs can't apply ? The actual solution drops logs every 30 days while over bookings can be higher than 30 days and up to 3 years.
- Why the support team need observability tools for such scenario ? To correlate telemetry easily when the issue is recent.
- Why the observability backend has been used to support this incompatible use case ? It was easy at the beginning when the total volume was low but due to high growth, it becomes incompatible.

## Make it simple
- __Too much logs__: How about keeping info/errors only ? It is still possible to activate debug logs temporary for a specific app. Configuration can be done at gateway / agent or app configuration.
- __Too much time series metrics__: How about aggregating more like one time serie per host to one time serie per env ?
- __Logs delivery__: If logs are important, how about using a true messaging-oriented middleware instead of logging libraries ?
- __Lost logs__: During communication failure between monitor apps and backend, without queues/disk buffer or gateway, logs are lost. How about introducing a true gateway/messaging-oriented middleware to fix that issue ?
- __Too much traces__: How about using tail sampling strategy + parent based to filter errors and latencies ?

## Conclusion
Keep observability solution as simple as possible by defining trade-offs which fit with observability tools, identify incompatibility and fix them by using appropriate ones.

Precision in telemetry is a hard topic and __monotonicity__ is a crucial topic for the next post.
