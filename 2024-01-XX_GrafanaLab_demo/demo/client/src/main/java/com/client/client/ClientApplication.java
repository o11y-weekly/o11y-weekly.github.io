package com.client.client;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.cloud.client.discovery.EnableDiscoveryClient;
import org.springframework.cloud.openfeign.EnableFeignClients;

import io.opentelemetry.exporter.otlp.http.logs.OtlpHttpLogRecordExporter;
import io.opentelemetry.exporter.otlp.http.logs.OtlpHttpLogRecordExporterBuilder;
import io.opentelemetry.instrumentation.logback.appender.v1_0.OpenTelemetryAppender;
import io.opentelemetry.sdk.OpenTelemetrySdk;
import io.opentelemetry.sdk.logs.SdkLoggerProvider;

@SpringBootApplication
@EnableFeignClients
@EnableDiscoveryClient
public class ClientApplication {

	public static void main(final String[] args) {
		final var openTelemetrySdk = OpenTelemetrySdk.builder()
				.setLoggerProvider(SdkLoggerProvider.builder().build()).build();

		OpenTelemetryAppender.install(openTelemetrySdk);

		final var exporter = OtlpHttpLogRecordExporter.getDefault();
		

		SpringApplication.run(ClientApplication.class, args);
	}

}
