#!/bin/sh
SCRIPT_DIR="$( cd -- "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"

if [ "$TERM" = "xterm" ]; then TERM=xterm-256color; fi

"$SCRIPT_DIR/bin/LocalPaper" "$@"
