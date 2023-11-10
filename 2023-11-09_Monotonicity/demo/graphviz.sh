#!/bin/bash
# https://github.com/pmsipilot/docker-compose-viz
# https://github.com/pmsipilot/docker-compose-viz#usage
docker run --rm -it --name dcv -v $(pwd):/input pmsipilot/docker-compose-viz render -m image compose.yml --no-volumes --no-networks --horizontal --force