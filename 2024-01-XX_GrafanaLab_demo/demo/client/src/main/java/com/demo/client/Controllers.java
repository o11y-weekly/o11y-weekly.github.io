package com.demo.client;

import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestController;

import com.demo.client.UsersServiceClient.User;

import io.opentelemetry.instrumentation.annotations.WithSpan;

import java.util.Random;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

@RestController
public record Controllers(UsersServiceClient client) {
	@ResponseStatus(value = HttpStatus.NOT_FOUND)
	private static class ResourceNotFoundException extends RuntimeException {
	}

	private static final Logger Logger = LoggerFactory.getLogger(Controllers.class);
	private static final Random random = new Random(0);

	@GetMapping("/")
	public String index() {
		return "Greetings from Spring Boot!";
	}

	private static int getRandom(int min, int max) {
		return random.nextInt(max - min + 1) + min;
	}

	@WithSpan
	private String getUser(final int id) {
		Logger.info("calling getUser");
		return client.getUser(id).map(user -> {
			final var ts = java.time.Instant.now().toEpochMilli();
			return "Hello " + user.name() + " " + user.surname() + " " + ts;
		}).orElseThrow(ResourceNotFoundException::new);
	}

	@GetMapping("/randomuser")
	public String getRandomUser() {
		return getUser(getRandom(1000, 1500));
	}

	@GetMapping("/user")
	public String getRandomUser(final @RequestParam("id") Integer id) {
		return getUser(id);
	}

	@PostMapping("/user")
	@ResponseStatus(value = HttpStatus.CREATED)
	public String createUser(final @RequestBody User user) {
		Logger.info("calling createUser");
		client.createUser(user);
		final var ts = java.time.Instant.now().toEpochMilli();
		return "created " + ts;
	}
}