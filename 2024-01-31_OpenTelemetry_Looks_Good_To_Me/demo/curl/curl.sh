#! /bin/sh

curl http://client:8080/randomuser \
&& curl http://client:8080/user?id=1 \
&& curl -X POST -H 'Content-Type: application/json' -d '{ "name": "hello", "surname": "world" }' -i client:8080/user