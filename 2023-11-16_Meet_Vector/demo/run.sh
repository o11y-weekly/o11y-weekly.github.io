#! /bin/bash
docker compose down --remove-orphans -v --rmi local && docker compose up