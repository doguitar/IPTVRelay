#!/bin/bash

umask $UMASK
cp -n /app/appsettings.json /data/appsettings.json
dotnet IPTVRelay.Blazor.dll -c /data
