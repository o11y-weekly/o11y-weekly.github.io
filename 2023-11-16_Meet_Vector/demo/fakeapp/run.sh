#!/bin/sh
while true
do
    DELAY=0.1
    echo "sleeping ${DELAY}s" && sleep ${DELAY}
    
    DATE=$(date +%Y%m%d)
    ./badlog.sh | tee -a ${LOG}/${DATE}.log
    ./log.sh | tee -a ${LOG}${DATE}.log
done