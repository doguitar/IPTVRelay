#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ./updogg/src/Updogg.Extensions/ /src/updogg/src/Updogg.Extensions/
COPY ./src /src/iptvrelay/

WORKDIR /src/iptvrelay/

RUN dotnet restore
RUN dotnet build "IPTVRelay.Blazor/IPTVRelay.Blazor.csproj" -c "$BUILD_CONFIGURATION" -o "/app/build" /nologo /clp:ErrorsOnly

FROM build AS publish
RUN dotnet publish "IPTVRelay.Blazor/IPTVRelay.Blazor.csproj" -c "$BUILD_CONFIGURATION" -o "/app/publish" /p:UseAppHost=false /nologo /clp:ErrorsOnly

FROM base AS final

ENV IPTV_CONFIGFOLDER="/data/"
ENV IPTV_DATAFOLDER="/data/"
ENV IPTV_PORT="8080"
ENV UID=99
ENV PID=100
ENV UMASK=022

WORKDIR /app
COPY --from=build "/src/iptvrelay/IPTVRelay.Blazor/entrypoint.sh" "/app"
COPY --from=publish /app/publish/ .

USER root
RUN chmod +x /app/entrypoint.sh
ENTRYPOINT ["/app/entrypoint.sh"]