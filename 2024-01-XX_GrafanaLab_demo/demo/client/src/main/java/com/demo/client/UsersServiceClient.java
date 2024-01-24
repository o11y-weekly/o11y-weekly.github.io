package com.demo.client;

import org.springframework.cloud.openfeign.FeignClient;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;

@FeignClient(name="service", path = "/user", url = "http://service:8080")
public interface UsersServiceClient {
    static record User(String name, String surname) {
    }
    @RequestMapping(method = RequestMethod.GET)
    User getUser(@RequestParam(value = "id") Integer id);
}