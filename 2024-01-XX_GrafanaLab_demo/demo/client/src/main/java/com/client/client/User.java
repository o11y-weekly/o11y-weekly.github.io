package com.client.client;

import lombok.Value;

@Value
public class User {
    private String name;
    private String surname;
    
    public User(final String name, final String surname) {
        this.name = name;
        this.surname = surname;
    }

    public String getName() {
        return name;
    }
    
    public String getSurname() {
        return surname;
    }
}