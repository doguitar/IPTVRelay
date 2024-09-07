#!/bin/bash
read -p "Enter migration name: " migrationName
dotnet ef migrations add $migrationName --project ./IPTVRelay.Database.Npgsql/ --context IPTVRelayNpgsqlContext --output-dir Migrations/Npgsql
dotnet ef migrations add $migrationName --project ./IPTVRelay.Database.Sqlite/ --context IPTVRelaySqliteContext --output-dir Migrations/Sqlite
