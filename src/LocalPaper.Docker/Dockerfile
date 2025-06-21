FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine

# Setup arguments
ARG UID=28433
ARG UNAME=localpaper

# Use root for setup
USER root

# Environment
ENV ENV="/etc/profile"
RUN echo "alias ll='ls -la'" > /etc/profile.d/00-aliases.sh

# Bash
RUN apk add bash
COPY src/LocalPaper.Docker/bash-default.sh /etc/bash/default.sh

# Helper packages
RUN apk add bind-tools tzdata

# SkiaSharp packages
RUN apk add fontconfig ttf-dejavu

# Healthcheck packages
RUN apk add curl

# Example
WORKDIR /app/example/any
COPY example/any/* ./

# Config
WORKDIR /config
RUN chown -R $UID:$UID /config

# Executables
WORKDIR /app
COPY bin/linux-musl-x64/LocalPaper      ./
COPY bin/linux-musl-x64/LocalPaper.pdb  ./
COPY bin/linux-musl-x64/libSkiaSharp.so ./
RUN chown -R $UID:$UID /app
RUN chmod +x LocalPaper

# User
RUN addgroup -g $UID $UNAME
RUN adduser -D -u $UID -G $UNAME $UNAME

# TcpDump (requires NET_ADMIN and NET_RAW)
RUN apk add tcpdump libcap
RUN setcap cap_net_raw,cap_net_admin=eip /usr/bin/tcpdump

# Docker
COPY src/LocalPaper.Docker/entrypoint.sh /entrypoint.sh
COPY src/LocalPaper.Docker/healthcheck.sh /healthcheck.sh
RUN chmod +x /entrypoint.sh /healthcheck.sh

# Done
USER $UNAME
HEALTHCHECK --start-period=10s --start-interval=5s  --interval=60s --retries=3 --timeout=5s CMD /healthcheck.sh
ENTRYPOINT [ "/entrypoint.sh" ]
