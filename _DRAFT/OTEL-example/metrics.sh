#! /bin/sh
curl -X POST -H 'Content-Type: application/json' -i localhost:4318/v1/metrics \
  -d "{
  \"resourceMetrics\": [{
    \"resource\": {
      \"attributes\": [{
        \"key\": \"service.name\",
        \"value\": {
          \"stringValue\": \"hello-world\"
        }
      }],
      \"droppedAttributesCount\": 0
    },
    \"scopeMetrics\": [{
      \"metrics\": [{
        \"description\": \"Example Counter\",
        \"name\": \"requests\",
        \"sum\": {
          \"aggregationTemporality\": 2,
          \"dataPoints\": [{
            \"asDouble\": 1,
            \"attributes\": [],
            \"startTimeUnixNano\": $(date +%s%N),
            \"timeUnixNano\": $(date +%s%N)
          }],
          \"isMonotonic\": true
        },
        \"unit\": \"1\"
      }]
    }]
  }]
}"