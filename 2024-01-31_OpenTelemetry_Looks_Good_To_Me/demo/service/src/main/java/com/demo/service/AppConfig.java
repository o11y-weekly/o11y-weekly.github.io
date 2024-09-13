package com.demo.service;

import org.hibernate.engine.spi.SessionFactoryImplementor;
import org.hibernate.stat.HibernateQueryMetrics;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

import io.micrometer.core.instrument.Tags;
import io.micrometer.core.instrument.binder.MeterBinder;
import io.micrometer.core.instrument.config.MeterFilter;
import jakarta.persistence.EntityManagerFactory;

@Configuration
public class AppConfig {
	@Bean
	MeterBinder hibernateQueryMetrics(EntityManagerFactory entityManager) {
		return new HibernateQueryMetrics(entityManager.unwrap(SessionFactoryImplementor.class), "mySess", Tags.empty());
	}

	@Bean
	public MeterFilter truncateFilter() {
		return MeterFilter.replaceTagValues("query", tagValue -> {
			final var maxLength = 1024;
			if (tagValue.length() > maxLength) {
				return String.format("%s...", tagValue.substring(0, maxLength - 3));
			}
			return tagValue;
		});
	}
}