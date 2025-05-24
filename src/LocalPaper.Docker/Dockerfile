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
RUN apk add bind-tools

# Executables
WORKDIR /app/bin
COPY bin/linux-musl-x64/LocalPaper      ./
COPY bin/linux-musl-x64/LocalPaper.pdb  ./
COPY bin/linux-musl-x64/libSkiaSharp.so ./
RUN  chmod +x LocalPaper

# Executables
WORKDIR /app
COPY src/LocalPaper.Docker/entrypoint.sh entrypoint
RUN chmod +x entrypoint

# Adjust permissions
RUN chown -R $UID:$UID /app

# User
RUN addgroup -g $UID $UNAME
RUN adduser -D -u $UID -G $UNAME $UNAME

# TcpDump (requires NET_ADMIN and NET_RAW)
RUN apk add tcpdump libcap
RUN setcap cap_net_raw,cap_net_admin=eip /usr/bin/tcpdump

# Done
USER $UNAME
ENTRYPOINT [ "/app/entrypoint" ]
