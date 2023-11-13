# 2023-11-16 #5 Meet Vector

This week, [Vector.dev](https://vector.dev/) from DataDog will be deeply analyzed.

## History

### Timeberio

### DataDog

### License

### When to use it ?
https://vector.dev/docs/setup/going-to-prod/architecting/#choosing-agents

## Hands-on

### Principle

Basic principle

Under the hood
https://vector.dev/docs/about/under-the-hood/

### Configuration
Organize files

### Transformation

Transformation Pipeline DAG : https://vector.dev/docs/about/under-the-hood/architecture/pipeline-model/

### Vector Remapping language

### Error Handling

### TDD

### Monitoring Vector

### Log to metric

## Conclusion

### Strength

+ Safe error handling
+ Documentation
+ Resiliency
+ Data Durability (buffer : https://vector.dev/docs/about/under-the-hood/architecture/buffering-model/#disk-buffers)

### Weekness

- OTLP support
- Datadog vendor locking : https://github.com/vectordotdev/vector/issues/1444#issuecomment-1704040812

### Alternatives

OpenTelemetry Collector Contrib