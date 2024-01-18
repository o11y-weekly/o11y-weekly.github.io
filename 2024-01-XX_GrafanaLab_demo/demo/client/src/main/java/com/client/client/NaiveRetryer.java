package com.client.client;

import org.springframework.stereotype.Component;

import feign.RetryableException;
import feign.Retryer;

@Component
public class NaiveRetryer implements feign.Retryer {
    @Override
    public void continueOrPropagate(final RetryableException e) {
        try {
            System.out.println("retrying...");
            Thread.sleep(1000L);
        } catch (InterruptedException ex) {
            Thread.currentThread().interrupt();
            throw e;
        }
    }
    @Override
    public Retryer clone() {
        return new NaiveRetryer();
    }
}