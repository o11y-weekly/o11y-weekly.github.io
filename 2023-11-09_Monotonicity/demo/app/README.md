# prometheus client example

This is a lightweight demo, this code is not production ready.
Pay close attention that the graceful shutdown is not properly implemented.

This implementations uses multiple ways including (otlp-http, prometheus scrap, statsd/udp, ...) to publish metrics and this is only for the sake of the demo.

reference : https://github.com/tikv/rust-prometheus/blob/master/examples/example_hyper.rs