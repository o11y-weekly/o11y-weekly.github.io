#! /bin/bash
docker compose down --remove-orphans -v --rmi local && docker compose up -d

while true 
do
    sleep 30
    docker compose kill app
    sleep 30
    docker compose start app
done