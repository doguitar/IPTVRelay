#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG UID
ARG PID
ARG UMASK

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS base
RUN apt-get update && apt-get install -y ffmpeg && apt-get clean && rm -rf /var/lib/apt/lists/*
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IPTVRelay.Blazor/IPTVRelay.Blazor/IPTVRelay.Blazor.csproj", "IPTVRelay.Blazor/IPTVRelay.Blazor/"]
COPY ["IPTVRelay.Database/IPTVRelay.Database.csproj", "IPTVRelay.Database/"]
COPY ["IPTVRelay.Library/IPTVRelay.Library.csproj", "IPTVRelay.Library/"]
COPY ["IPTVRelay.Blazor/IPTVRelay.Blazor.Client/IPTVRelay.Blazor.Client.csproj", "IPTVRelay.Blazor/IPTVRelay.Blazor.Client/"]
RUN dotnet restore "./IPTVRelay.Blazor/IPTVRelay.Blazor/./IPTVRelay.Blazor.csproj"
COPY . .
WORKDIR "/src/IPTVRelay.Blazor/IPTVRelay.Blazor"
RUN dotnet build "./IPTVRelay.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IPTVRelay.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

ENV IPTV_DATA_FOLDER="/data"
ENV IPTV_PORT="8080"
ENV UID=$UID
ENV PID=$PID
ENV UMASK=$UMASK


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $UID:$PID
COPY ["IPTVRelay.Blazor/IPTVRelay.Blazor/entrypoint.sh", "/app/entrypoint.sh"]
RUN chmod +x /app/entrypoint.sh
ENTRYPOINT ["/app/entrypoint.sh"]