#!/bin/sh
while true
do
    DELAY=0.1
    echo "sleeping ${DELAY}s" && sleep ${DELAY}
    
    DATE=$(date +%Y%m%d)
    LOG_PATH=${LOG}/$(hostname)_${DATE}.log
    ./badlog.sh | tee -a ${LOG_PATH}
    ./log.sh | tee -a ${LOG_PATH}
done