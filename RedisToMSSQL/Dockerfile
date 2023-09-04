FROM mcr.microsoft.com/dotnet/sdk:7.0.203-alpine3.17-amd64 AS builder

RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app
COPY . .

RUN dotnet build RedisToMSSQL.sln -o /usr/src/app/bin/
RUN ls -l /usr/src/app/bin/
RUN cd /usr/src/app/bin && tar czf apps_exe.tar.gz .

FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-alpine3.17-amd64 AS deployer

RUN apk update 
RUN apk add --no-cache procps net-tools bash
RUN apk add --no-cache tzdata git vim
RUN apk add --no-cache curl wget
# RUN apk add --upgrade supervisor
## ref.: https://www.cnblogs.com/straycats/p/13709000.html
RUN apk add --no-cache icu-libs
RUN rm -rf /var/cache/apk/*

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /usr/local/bin
COPY --from=builder /usr/src/app/bin/apps_exe.tar.gz /usr/local/bin/
RUN cd /usr/local/bin/ && tar xzf apps_exe.tar.gz
RUN rm /usr/local/bin/apps_exe.tar.gz

RUN chmod -R 755 /usr/local/bin

CMD [ "/usr/bin/dotnet", "/usr/local/bin/RedisToMSSQL.dll", "-entry /usr/local/bin" ]
