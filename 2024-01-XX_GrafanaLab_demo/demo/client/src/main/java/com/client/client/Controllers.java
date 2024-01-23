package com.client.client;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

import io.opentelemetry.instrumentation.annotations.WithSpan;

import java.util.Random;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

@RestController
public class Controllers {
	private static final Logger Logger = LoggerFactory.getLogger(Controllers.class);
	private static final Random random = new Random(0);

	@Autowired
	UsersServiceClient client;	

	@GetMapping("/")
	public String index() {
		return "Greetings from Spring Boot!";
	}

	private static int getRandom(int min, int max) {
		return random.nextInt(max - min + 1) + min;
	}

	@WithSpan
	private String getHello() {
		Logger.info("calling getUser");
		final var id = getRandom(1000, 1500); 

		final var user = client.getUser(id);
		final var ts = java.time.Instant.now().toEpochMilli();
		return "Hello " + user.name() + " " + user.surname() + " " + ts;
	}

	@GetMapping("/hello")
	public String hello() {
		Logger.info("/hello has been called");
		return getHello();
	}
}