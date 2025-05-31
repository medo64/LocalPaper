#!/bin/sh
SCRIPT_DIR="$( cd -- "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"

if [ "$TERM" = "xterm" ]; then TERM=xterm-256color; fi

if [ -z "$(ls -A "/config")" ]; then
    cp -r "$SCRIPT_DIR/example/"* "/config/" 2>/dev/null || true
    if [ ! -z "$(ls -A "/config")" ]; then
        echo "Copied example configuration to /config."
    fi
fi

"$SCRIPT_DIR/bin/LocalPaper" "$@"
