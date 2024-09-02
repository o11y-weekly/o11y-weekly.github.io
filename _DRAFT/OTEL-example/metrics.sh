#! /bin/sh
curl -v -X POST -H 'Content-Type: application/json' http://localhost:4318/v1/metrics \
  -d "{
  \"resourceMetrics\": [{
    \"resource\": {
      \"attributes\": [{
        \"key\": \"service.name\",
        \"value\": {
          \"stringValue\": \"hello-world\"
        }
      },
      {
        \"key\": \"hello\",
        \"value\": {
          \"stringValue\": \"world\"
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
            \"attributes\": [{
                \"key\": \"hello-dp\",
                \"value\": {
                \"stringValue\": \"world\"
                }
            }],
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