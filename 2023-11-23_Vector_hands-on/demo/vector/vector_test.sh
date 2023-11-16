#! /bin/bash

if docker run --rm -w /vector -v $(pwd):/vector/config/ timberio/vector:0.34.0-debian test --config-toml /vector/config/**/*.toml; then
    echo "all tests ... passed"
else
    echo "test failed! exit code : $?"
fi