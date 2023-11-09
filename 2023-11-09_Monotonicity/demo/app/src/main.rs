use hyper::{
    service::{make_service_fn, service_fn},
    Body, Request, Response, Server,
};
use opentelemetry_otlp::WithExportConfig;
use prometheus_client::{
    encoding::text::encode, metrics::counter::Counter as PrometheusCounter, registry::Registry,
};
use std::{
    future::Future,
    io,
    net::{IpAddr, Ipv4Addr, SocketAddr, UdpSocket},
    pin::Pin,
    sync::Arc,
    time::Duration,
};
use tokio::signal;

use opentelemetry::{
    global,
    metrics::{self, Counter as OtlpCounter},
    KeyValue,
};
use opentelemetry_sdk::metrics as sdkmetrics;

fn init_otlp_metrics(endpoint: &str) -> metrics::Result<sdkmetrics::MeterProvider> {
    let export_config = opentelemetry_otlp::ExportConfig {
        endpoint: endpoint.to_owned(),
        protocol: opentelemetry_otlp::Protocol::HttpBinary,
        timeout: Duration::from_secs(30),
    };
    opentelemetry_otlp::new_pipeline()
        .metrics(opentelemetry_sdk::runtime::Tokio)
        .with_exporter(
            opentelemetry_otlp::new_exporter()
                .tonic()
                .with_export_config(export_config),
        )
        .with_period(Duration::from_secs(10))
        .build()
}

#[tokio::main]
async fn main() {
    let _ = init_otlp_metrics("http://otelcollector:4317").unwrap();
    let name = "requests";
    let description = "How many requests the application has received";
    let meter = global::meter(name);
    let otlp_counter = meter
        .u64_counter("foo")
        .with_description(description)
        .init();

    let socket = UdpSocket::bind("0.0.0.0:0").unwrap();
    let prometheus_counter: PrometheusCounter<u64> = Default::default();

    let mut registry = <Registry>::with_prefix("foo");

    registry.register(name, description, prometheus_counter.clone());

    // Spawn a server to serve the OpenMetrics endpoint.
    let metrics_addr = SocketAddr::new(IpAddr::V4(Ipv4Addr::new(0, 0, 0, 0)), 8001);
    start_metrics_server(
        metrics_addr,
        registry,
        prometheus_counter,
        otlp_counter,
        Arc::new(socket),
    )
    .await
}

/// Start a HTTP server to report metrics.
pub async fn start_metrics_server(
    metrics_addr: SocketAddr,
    registry: Registry,
    prometheus_counter: PrometheusCounter<u64>,
    otlp_counter: OtlpCounter<u64>,
    socket: Arc<UdpSocket>,
) {
    eprintln!("Starting metrics server on {metrics_addr}");

    let registry = Arc::new(registry);
    Server::bind(&metrics_addr)
        .serve(make_service_fn(move |_conn| {
            let registry = registry.clone();
            let prometheus_counter = prometheus_counter.clone();
            let otlp_counter = otlp_counter.clone();
            let socket = socket.clone();
            async move {
                let handler = make_handler(registry, prometheus_counter, otlp_counter, socket);
                Ok::<_, io::Error>(service_fn(handler))
            }
        }))
        .with_graceful_shutdown(async move {
            match signal::ctrl_c().await {
                Ok(()) => {}
                Err(err) => {
                    eprintln!("Unable to listen for shutdown signal: {}", err);
                    // we also shut down in case of error
                }
            }
        })
        .await
        .unwrap();
}

/// This function returns a HTTP handler (i.e. another function)
pub fn make_handler(
    registry: Arc<Registry>,
    prometheus_counter: PrometheusCounter<u64>,
    otlp_counter: OtlpCounter<u64>,
    socket: Arc<UdpSocket>,
) -> impl Fn(Request<Body>) -> Pin<Box<dyn Future<Output = io::Result<Response<Body>>> + Send>> {
    // This closure accepts a request and responds with the OpenMetrics encoding of our metrics.
    move |_req: Request<Body>| {
        prometheus_counter.inc();
        otlp_counter.add(
            1,
            &[
                KeyValue::new("v", "1"),
                KeyValue::new("service.name", "app"),
            ],
        );
        socket.send_to(b"foo:1|c", "graphite:8125").unwrap();

        let reg = registry.clone();
        Box::pin(async move {
            let mut buf = String::new();
            encode(&mut buf, &reg.clone())
                .map_err(|e| std::io::Error::new(std::io::ErrorKind::Other, e))
                .map(|_| {
                    let body = Body::from(buf);
                    Response::builder()
                        .header(
                            hyper::header::CONTENT_TYPE,
                            "application/openmetrics-text; version=1.0.0; charset=utf-8",
                        )
                        .body(body)
                        .unwrap()
                })
        })
    }
}
