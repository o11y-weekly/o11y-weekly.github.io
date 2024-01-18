package com.client.client;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

import io.opentelemetry.instrumentation.annotations.WithSpan;

import lombok.RequiredArgsConstructor;

import java.util.Random;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

@RestController
@RequiredArgsConstructor
public class Controllers {
	private static final Logger Logger = LoggerFactory.getLogger(Controllers.class);
	private static final Random random = new Random(0);
	private static final Logger customLogger = LoggerFactory.getLogger("custom_logger");

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
		int hotel = getRandom(1000, 1500); 
		int timing = getRandom(100, 10000);

		User user = client.getUser(hotel);
		long ts = java.time.Instant.now().toEpochMilli();
		customLogger.info("H={}\tT={}", hotel, timing);
		customLogger.info("bad log");
		return "Hello " + user.getName() + " " + user.getSurname() + " " + ts;
	}

	@GetMapping("/hello")
	public String hello() {
		Logger.info("/hello has been called");
		return getHello();
	}
}