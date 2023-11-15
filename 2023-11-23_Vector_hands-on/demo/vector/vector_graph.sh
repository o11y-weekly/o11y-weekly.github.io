#! /bin/bash
docker run --rm -w /vector -v $(pwd):/vector/config/ timberio/vector:0.34.0-debian graph --config-toml /vector/config/**/*.toml | dot -Tsvg > graph.svg