#!/bin/sh
echo "t=$(date +%s%N)	H=$(shuf -i 1000-9999 -n 1)	T=$(shuf -i 10-15000 -n 1)"