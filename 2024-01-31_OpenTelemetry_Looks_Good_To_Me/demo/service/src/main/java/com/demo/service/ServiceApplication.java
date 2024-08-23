package com.demo.service;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;

@SpringBootApplication
@EnableJpaRepositories(basePackages = "com.demo.service")
public class ServiceApplication {

	public static void main(final String[] args) {
		SpringApplication.run(ServiceApplication.class, args);
	}

}
