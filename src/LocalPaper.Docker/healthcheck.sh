#!/bin/sh

ANSI_RESET="\e[0m"
ANSI_RED="\e[91m"
ANSI_GREEN="\e[92m"

HTTP_ANSWER=`curl -Is http://127.0.0.1/health 2>/dev/null | head -1`
HTTP_STATUS=`echo $HTTP_ANSWER | awk '{print $2}'`

if [ "$HTTP_STATUS" = "200" ]; then
    echo -e "${ANSI_GREEN}Healthy${ANSI_RESET}"
    exit 0
else
    echo -e "${ANSI_RED}Not healthy${ANSI_RESET}"
    exit 1
fi
