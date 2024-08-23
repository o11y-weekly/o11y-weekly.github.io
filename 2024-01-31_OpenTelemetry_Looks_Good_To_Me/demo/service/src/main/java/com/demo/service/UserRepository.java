package com.demo.service;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.demo.service.Controllers.UserRecord;

@Repository
public interface UserRepository extends JpaRepository<UserRecord, Long> {
}