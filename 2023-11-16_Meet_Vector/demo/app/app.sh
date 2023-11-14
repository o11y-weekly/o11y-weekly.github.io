#!/bin/sh
while true
do
    DELAY=0.1
    echo "sleeping ${DELAY}s" && sleep ${DELAY}
    
    LOG_PATH=${LOG_BASE_PATH}/$(hostname)_$(date +%Y%m%d).log
    ./badlog.sh | tee -a ${LOG_PATH}
    ./log.sh | tee -a ${LOG_PATH}
done