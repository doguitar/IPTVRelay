@echo off
set /p migrationName="Enter migration name: "
dotnet ef migrations add %migrationName% --project .\IPTVRelay.Database.Psql\ --context IPTVRelayNpgsqlContext -- --psqlhost "10.10.1.10" --psqlport 5432 --psqldb  iptvrelay --psqluser postgres --psqlpass 9p6qetas
dotnet ef migrations add %migrationName% --project .\IPTVRelay.Database.Sqlite\ --context IPTVRelaySqliteContext 
