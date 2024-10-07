#!/bin/bash

umask $UMASK

mkdir -p "$IPTV_CONFIGFOLDER"
mkdir -p "$IPTV_DATAFOLDER"

if getent group $GID > /dev/null 2>&1; then
    group=$(getent group $GID | cut -d: -f1)
else
    groupadd -g $GID iptvgroup
    group="iptvgroup"
fi

if id -u $UID > /dev/null 2>&1; then
    user=$(id -nu $UID)
else
    useradd iptvuser -u $UID -g $GID -M
    user="iptvuser"
fi

chown -R $UID:$GID "$IPTV_CONFIGFOLDER"
chown -R $UID:$GID "$IPTV_DATAFOLDER"

chmod -R ug+rw "$IPTV_CONFIGFOLDER"
chmod -R ug+rw "$IPTV_DATAFOLDER"

cp -n /app/appsettings.json "$IPTV_CONFIGFOLDER"


export UMASK=$UMASK
export UID=$UID
export GID=$GID
export IPTV_CONFIGFOLDER=$IPTV_CONFIGFOLDER
export IPTV_DATAFOLDER=$IPTV_DATAFOLDER
export IPTV_PORT=$IPTV_PORT

exec su -s /bin/bash -c "/usr/bin/dotnet /app/IPTVRelay.Blazor.dll -c \"$IPTV_CONFIGFOLDER\"" "$user"
#sudo -u $UID dotnet IPTVRelay.Blazor.dll -c "$IPTV_CONFIGFOLDER"
#dotnet IPTVRelay.Blazor.dll -c "$IPTV_CONFIGFOLDER"
