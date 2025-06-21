#!/bin/sh

if [ "$TERM" = "xterm" ]; then TERM=xterm-256color; fi

exec 28433> /var/tmp/.entrypoint.lock
flock -n 28433 || { echo -e "\033[91mScript is already running\033[0m" >&2 ; exit 113; }

if [ -z "$(ls -A "/config")" ]; then
    cp -r "/app/example/"* "/config/" 2>/dev/null || true
    if [ ! -z "$(ls -A "/config")" ]; then
        echo "Copied example configuration to /config."
    fi
fi

/app/LocalPaper "$@"
