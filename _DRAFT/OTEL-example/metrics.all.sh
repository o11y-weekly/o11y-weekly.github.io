#! /bin/sh
curl -v -X POST -H 'Content-Type: application/json' http://localhost:4318/v1/metrics \
  -d "{
  \"resourceMetrics\": [
    {
      \"resource\": {
        \"attributes\": [
          {
            \"key\": \"service.name\",
            \"value\": {
              \"stringValue\": \"my.service\"
            }
          }
        ]
      },
      \"scopeMetrics\": [
        {
          \"scope\": {
            \"name\": \"my.library\",
            \"version\": \"1.0.0\",
            \"attributes\": [
              {
                \"key\": \"my.scope.attribute\",
                \"value\": {
                  \"stringValue\": \"some scope attribute\"
                }
              }
            ]
          },
          \"metrics\": [
            {
              \"name\": \"my.counter\",
              \"unit\": \"1\",
              \"description\": \"I am a Counter\",
              \"sum\": {
                \"aggregationTemporality\": 1,
                \"isMonotonic\": true,
                \"dataPoints\": [
                  {
                    \"asDouble\": 5,
                    \"startTimeUnixNano\": \"$(date +%s%N)\",
                    \"timeUnixNano\": \"$(date +%s%N)\",
                    \"attributes\": [
                      {
                        \"key\": \"my.counter.attr\",
                        \"value\": {
                          \"stringValue\": \"some value\"
                        }
                      }
                    ]
                  }
                ]
              }
            },
            {
              \"name\": \"my.gauge\",
              \"unit\": \"1\",
              \"description\": \"I am a Gauge\",
              \"gauge\": {
                \"dataPoints\": [
                  {
                    \"asDouble\": 10,
                    \"timeUnixNano\": \"$(date +%s%N)\",
                    \"attributes\": [
                      {
                        \"key\": \"my.gauge.attr\",
                        \"value\": {
                          \"stringValue\": \"some value\"
                        }
                      }
                    ]
                  }
                ]
              }
            },
            {
              \"name\": \"my.histogram\",
              \"unit\": \"1\",
              \"description\": \"I am a Histogram\",
              \"histogram\": {
                \"aggregationTemporality\": 1,
                \"dataPoints\": [
                  {
                    \"startTimeUnixNano\": \"$(date +%s%N)\",
                    \"timeUnixNano\": \"$(date +%s%N)\",
                    \"count\": 2,
                    \"sum\": 2,
                    \"bucketCounts\": [1,1],
                    \"explicitBounds\": [1],
                    \"min\": 0,
                    \"max\": 2,
                    \"attributes\": [
                      {
                        \"key\": \"my.histogram.attr\",
                        \"value\": {
                          \"stringValue\": \"some value\"
                        }
                      }
                    ]
                  }
                ]
              }
            }
          ]
        }
      ]
    }
  ]
}
"