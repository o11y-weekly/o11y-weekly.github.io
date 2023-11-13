#! /bin/sh
echo "t=$(date +%Y-%m-%dT%H:%M:%S.%N%:z)	h=$(hostname)	H=$(shuf -i 1000-9999 -n 1)	T=$(shuf -i 10-15000 -n 1)"