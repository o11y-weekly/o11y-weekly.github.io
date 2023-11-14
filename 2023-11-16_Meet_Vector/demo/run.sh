#! /bin/bash
docker compose down --remove-orphans -v --rmi local && docker compose up -d

docker compose logs -f &

while true 
do
    sleep 30
    docker compose restart vector
done