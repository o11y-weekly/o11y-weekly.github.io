# 2023-11-23 #6 Vector in action

<iframe src="https://youtu.be/gYoY9NCajbE?si=JZ3jjXYX82qB1M40" title="Vector.dev Log_to_metric demo" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

[Last week, vector has been introduced](../2023-11-16_Meet_Vector/README.md), and this week, this demo has been created with a complete vector demo including log to metric, monitoring vector agent and host metrics (Vector as a node exporter) has been setup into the demo.

## Run the demo

1/ Clone the [repository](./)
- https://github.com/o11y-weekly/o11y-weekly.github.io/tree/main/2023-11-23_Vector_in_action/demo

2/ Run the script
```bash
./run.sh
```

3/ Open Grafana Dashboards:
  - [Log to metric](http://localhost:3000/d/eEZIy984z/log-2-metric?orgId=1&refresh=5s)
  - [Agent Monitoring](http://localhost:3000/d/eEZIy984z/log-2-metric?orgId=1&refresh=5s) : monitoring vector and pipelines
  - [Vector host_metrics](http://localhost:3000/d/rYdddlPWk/node-exporter-vector-host-metrics?orgId=1&refresh=5s): setup vector to replace a node exporter agent.

### Demo Components

![Architecture Demo](./docker-compose.png)

The app contains 2 [supervisor](http://supervisord.org/) services: 
- [app.sh](./app/app.sh) writes logs to LOG_BASE_PATH (/workspace/app/log/). The log structure is 2 lines, bad logs will be use to monitor pipeline errors while the second one will be parsed as a metric (H is hotel and T in the timing part in milliseconds).
 ```bash
t=2023-11-20T11:34:15.692975421+00:00 bad logs
t=2023-11-20T11:34:15.694559072+00:00 H=2497  T=2725
```
- [vector](./app/supervisor/supervisor.d/vector.ini) reads the logs from the app and converts logs to metrics.

A mimir, loki and grafana will be used as a backend to visualize datapoints.

![Datapoints](./Log2Metrics_Dashboard.png)

## Log to metric convertion

To convert log to metrics, the log should be structured and correctly parsed. Vector component [Vector Remapping Language](https://vector.dev/docs/reference/vrl/) is used to parse line to structured log.

[VRL](https://vector.dev/docs/reference/vrl/) is a lightweight [rust](https://www.rust-lang.org/) which brings [safety](https://vector.dev/docs/reference/vrl/#safety) and true error handling to avoid runtime error when the error can be statically verifier during writing the parser.

### Vector Remapping Language (VRL)
```vrl
. |= parse_key_value!(.message, field_delimiter:"\t", accept_standalone_key:false)
.timestamp = parse_timestamp!(.t, "%Y-%m-%dT%H:%M:%S%.f%:z")
.job = "vector"
del(.message)
```

`.message` is the raw message which can be parsed.

1/ parse message as key value and put the object at root level (dot) `.`. The parse_key_value returns a result which can be the object or an error. The bang `!` operator is used to fail on error.

2/ Add a field `timestamp` by parsing and fail if there is an error

3/ Add `job` field to vector

4/ Remove `message` to avoid paying twice raw and structured signal.

/!\ This program will fail on error meaning that vector will stop on error.

It is also possible to handle errors in vrl but it comes at repeating the same error handling every time.

Instead of failing vector on error, it is also possible and recommended to drop on error so that vector will not fail at all and the message is not lost but rerouted.

### Log to Metric Transformation
#### Pipeline setup
Log to metric transformation in vector is a common pattern where logs are visualized as aggregated metrics only. The problem of visualizing logs can be the required percentage of CPU or simply the cost of long term retention higher than one year based on logs.

To avoid high ressource usage to simply visualize datapoints, a lot to metric transformation can be used to convert log to metric.

The demo log to metric pipeline: 

1/ Setup source app_file_raw

2/ Parse line to structured log by configuring the transforms conversion by using vector remapping language (VRL) [keyvalue.vrl](./vector/vrl/keyvalue.vrl). Drop on error and abort and reroute thoses messages to a drop channel.

3/ Setup the `log_to_metric` vector transforms by creating 2 counters: one incremented by log (`app.count`) and another one incremented by `T` value (`app.total`) which is the thinktime of the `app` component.

```toml
# file log
[sources.applog_file_raw]
type = "file"
include = ["${LOG_BASE_PATH}**/*.*"]

[transforms.applog_file]
type = "remap"
inputs = ["applog_file_raw"]
file = "config/vrl/keyvalue.vrl"
# forward to a dead letter queue on error or abort 
drop_on_error = true
drop_on_abort = true
reroute_dropped = true

[transforms.applog_file_2_metric]
type = "log_to_metric"
inputs = ["applog_file"]
[[transforms.applog_file_2_metric.metrics]]
type = "counter"
field = "T"
namespace = "app"
name = "count"
    [transforms.applog_file_2_metric.metrics.tags]
    hotel = "{{H}}"
[[transforms.applog_file_2_metric.metrics]]
type = "counter"
field = "T"
namespace = "app"
name = "total"
increment_by_value = true
    [transforms.applog_file_2_metric.metrics.tags]
    hotel = "{{H}}"
```

#### Vector test
Vector has the ability to unit test the transformation pipeline like the log 2 metric test.

1/ Substitute the input `applog_file` : the structured log.

2/ Inject an entry `.message =2023-11-13T15:53:37.728584030+01:00\th=FR-LT-00410\tH=6666\tT=5663`

3/ Intercept the pipeline output of `applog_file` to `test.outputs`

4/ Assert the output with a [vrl]((https://vector.dev/docs/reference/vrl/)) assertion `assert!(is_timestamp(.timestamp))`.

5/ Run the unit test

    ```bash
    docker run --rm -w /vector -v $(pwd):/vector/config/ timberio/vector:0.34.0-debian test --config-toml /vector/config/**/*.toml
    ```

A [vector_test.sh](./vector/vector_test.sh) script has been made to ease the vector integration.

```toml
[[tests]]
name = "Test applog_file parsing"

[[tests.inputs]]
insert_at = "applog_file"
type = "log" 

[tests.inputs.log_fields]
message = "t=2023-11-13T15:53:37.728584030+01:00\th=FR-LT-00410\tH=6666\tT=5663"

[[tests.outputs]]
extract_from = "applog_file"

[[tests.outputs.conditions]]
type = "vrl"
source = '''
assert!(is_timestamp(.timestamp))
'''
```

#### Vector graph

Vector has a `graph` command which produces dot graph. A [vector_graph.sh](./vector/vector_graph.sh) is available to build this graph.

```bash
docker run --rm -w /vector -v $(pwd):/vector/config/ timberio/vector:0.34.0-debian graph --config-toml /vector/config/**/*.toml | dot -Tsvg > graph.svg
```

![log to metric pipeline](./graph.svg)

## Vector monitoring

## Vector as Node exporter