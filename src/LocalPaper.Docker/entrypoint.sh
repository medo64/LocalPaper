#!/bin/sh
SCRIPT_NAME=`basename $0`

if [ "$TERM" = "xterm" ]; then TERM=xterm-256color; fi

PID_FILE="/var/run/.$SCRIPT_NAME.pid"
if [ -e "$PID_FILE" ]; then
    PID_LAST=$(cat "$PID_FILE")
    if ps -axo pid | grep -q "^ *$PID_LAST\$"; then
        echo "${ANSI_RED}$SCRIPT_NAME: script is already running!${ANSI_RESET}" >&2
        exit 255
    fi
fi
echo $$ > $PID_FILE
trap "rm $PID_FILE 2>/dev/null" 0
trap "exit 113" INT TERM

if [ -z "$(ls -A "/config")" ]; then
    cp -r "/app/example/"* "/config/" 2>/dev/null || true
    if [ ! -z "$(ls -A "/config")" ]; then
        echo "Copied example configuration to /config."
    fi
fi

/app/LocalPaper "$@"
