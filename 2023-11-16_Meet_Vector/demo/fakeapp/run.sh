#!/bin/sh
while true
do
    DELAY=$(seq 0 .001 2 | shuf -n1)
    echo "sleeping ${DELAY}s" && sleep ${DELAY}
    
    DATE=$(date +%Y%m%d)
    ./badlog.sh | tee ${LOG}/${DATE}.log
    ./log.sh | tee ${LOG}${DATE}.log
done