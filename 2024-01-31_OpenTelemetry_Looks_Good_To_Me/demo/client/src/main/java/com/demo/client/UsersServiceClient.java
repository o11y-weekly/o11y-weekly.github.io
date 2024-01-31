package com.demo.client;

import java.util.Optional;

import org.springframework.cloud.openfeign.FeignClient;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;

@FeignClient(name="service", path = "/user", url = "http://service:8080", dismiss404=true)
public interface UsersServiceClient {
    static record User(String name, String surname) {
    }
    @RequestMapping(method = RequestMethod.GET)
    Optional<User> getUser(@RequestParam(value = "id") Integer id);

    @RequestMapping(method = RequestMethod.POST)
    void createUser(@RequestBody User user);
}