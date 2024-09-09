package com.demo.client;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.ImportAutoConfiguration;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.context.properties.ConfigurationPropertiesScan;
import org.springframework.cloud.openfeign.EnableFeignClients;

import com.demo.client.otel.OpenTelemetryLoggingAutoConfiguration;


@SpringBootApplication
@EnableFeignClients
@ConfigurationPropertiesScan("com.demo.client.otel")
@ImportAutoConfiguration(OpenTelemetryLoggingAutoConfiguration.class)
public class ClientApplication {

	public static void main(final String[] args) {
		SpringApplication.run(ClientApplication.class, args);
	}

}
