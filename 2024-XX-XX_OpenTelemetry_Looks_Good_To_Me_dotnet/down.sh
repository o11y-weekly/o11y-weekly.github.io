#!/bin/bash
set -eux
. ./env.sh

docker compose down --remove-orphans -v --rmi local