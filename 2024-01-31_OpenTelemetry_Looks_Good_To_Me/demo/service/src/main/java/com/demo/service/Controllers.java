package com.demo.service;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.jdbc.core.RowMapper;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestController;

import io.opentelemetry.instrumentation.annotations.WithSpan;
import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.List;
import java.util.Optional;
import java.util.Random;
import java.util.concurrent.atomic.AtomicInteger;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

@RestController
public record Controllers(@Autowired JdbcTemplate jdbcTemplate, @Autowired UserRepository userRepository) {
	private static record User(String name, String surname) {
	}
	@Entity(name="persons")
	static class UserRecord {
		@Id
		@GeneratedValue
		private Long id;

		private String firstname;

		private String surname;
	}

	private static class UserMapper implements RowMapper<User> {
		@Override
		public User mapRow(final ResultSet rs, final int rowNum) throws SQLException {
			return new User(rs.getString("firstname"), rs.getString("surname"));
		}
	}

	@ResponseStatus(value = HttpStatus.NOT_FOUND)
	private static class ResourceNotFoundException extends RuntimeException {
	}

	private static final Logger logger = LoggerFactory.getLogger(Controllers.class);
	private static final Random random = new Random(0);

	private static final AtomicInteger COUNTER = new AtomicInteger(0);

	private static final UserMapper USER_MAPPER = new UserMapper();

	private static final Double FAILURE_RATIO = Optional.ofNullable(System.getenv("FAILURE_RATIO"))
			.map(Double::parseDouble).orElse(0.);
	private static final Double LATENCY_RATIO = Optional.ofNullable(System.getenv("LATENCY_RATIO"))
			.map(Double::parseDouble).orElse(0.);
	private static final Integer MIN_LATENCY = Optional.ofNullable(System.getenv("MIN_LATENCY")).map(Integer::parseInt)
			.orElse(0);
	private static final Integer MAX_LATENCY = Optional.ofNullable(System.getenv("MAX_LATENCY")).map(Integer::parseInt)
			.orElse(0);

	@WithSpan
	private Optional<User> getUserFromDatabase(final int id) {
		logger.info("getting user {} from database", id);
		return jdbcTemplate.query("select firstname, surname from persons where id=?", USER_MAPPER, id).stream()
				.findFirst();
	}

	private Optional<User> getUserFromHibernate(final int id) {
		// FIXME: Why findById does not add metrics (JdbcSelectExecutorStandardImpl.class L218 stats boolean) ? => SingleIdPlanLoader has stats bool to false while the findAll* not.
		// final var user = userRepository.findById((long) id);
		final var user = userRepository.findAllById(List.of((long)id));
		// final var user = userRepository.findAll().stream().filter(x -> x.id.equals((long)id)).findFirst();
		return user.stream().map(x -> new User(x.firstname, x.surname)).findFirst();
	}

	@WithSpan
	private User getFailedUserFromDatabase(final int id) {
		logger.info("(will fail) getting user {} from database", id);
		return jdbcTemplate.queryForObject("select firstname, surname from bad_table where id=?", USER_MAPPER, id);
	}

	@WithSpan
	private void createUserIntoDatabase(final User user) {
		jdbcTemplate.update("insert into persons(firstname, surname) values(?, ?)", user.name, user.surname);
	}

	private static int getRandom(final int min, final int max) {
		return random.nextInt(max - min + 1) + min;
	}

	@WithSpan
	private static void callSlowDependency(final int counter, final int latency) throws InterruptedException {
		if (isEnabled(counter, LATENCY_RATIO)) {
			Thread.sleep(latency);
		}
	}

	@GetMapping(path = "/user")
	User getUser(final @RequestParam("id") Integer id) throws InterruptedException {
		final var counter = COUNTER.getAndIncrement();
		final var timing = getRandom(MIN_LATENCY, MAX_LATENCY);

		callSlowDependency(counter, timing);
		logger.info("/user has been called!");
		if (isEnabled(counter, FAILURE_RATIO)) {
			return getFailedUserFromDatabase(id);
		}

		return getUserFromHibernate(id).orElseThrow(ResourceNotFoundException::new);
	}

	private static boolean isEnabled(final int counter, final double ratio) {
		return counter % (1 / ratio) == 0;
	}

	@PostMapping(path = "/user")
	@ResponseStatus(value = HttpStatus.CREATED)
	void createUser(final @RequestBody User user) {
		createUserIntoDatabase(user);
	}
}