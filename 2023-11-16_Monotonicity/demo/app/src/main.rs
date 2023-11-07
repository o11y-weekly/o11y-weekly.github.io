use hyper::{
    service::{make_service_fn, service_fn},
    Body, Request, Response, Server,
};
use prometheus_client::{encoding::text::encode, metrics::counter::Counter, registry::Registry};
use std::{
    future::Future,
    io,
    net::{IpAddr, Ipv4Addr, SocketAddr},
    pin::Pin,
    sync::Arc,
};
use tokio::signal;

#[tokio::main]
async fn main() {
    let request_counter: Counter<u64> = Default::default();

    let mut registry = <Registry>::with_prefix("tokio_hyper_example");

    registry.register(
        "requests",
        "How many requests the application has received",
        request_counter.clone(),
    );

    // Spawn a server to serve the OpenMetrics endpoint.
    let metrics_addr = SocketAddr::new(IpAddr::V4(Ipv4Addr::new(0, 0, 0, 0)), 8001);
    start_metrics_server(metrics_addr, registry, request_counter).await
}

/// Start a HTTP server to report metrics.
pub async fn start_metrics_server(
    metrics_addr: SocketAddr,
    registry: Registry,
    counter: Counter<u64>,
) {
    eprintln!("Starting metrics server on {metrics_addr}");

    let registry = Arc::new(registry);
    Server::bind(&metrics_addr)
        .serve(make_service_fn(move |_conn| {
            let registry = registry.clone();
            let counter = counter.clone();
            async move {
                let handler = make_handler(registry, counter);
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
    counter: Counter<u64>,
) -> impl Fn(Request<Body>) -> Pin<Box<dyn Future<Output = io::Result<Response<Body>>> + Send>> {
    // This closure accepts a request and responds with the OpenMetrics encoding of our metrics.
    move |_req: Request<Body>| {
        counter.inc();
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
