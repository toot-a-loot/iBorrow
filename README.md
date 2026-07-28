1.) Must have postgresql

2.) run user secrets. example:
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=iborrow;Username=<their_pg_user>;Password=<their_pg_pass>"
dotnet user-secrets set "Seed:AdminEmail" "admin@iborrow.com"
dotnet user-secrets set "Seed:AdminPassword" "admin1234"

Note: No separate dotnet ef database update needed: DbSeeder.cs:17 calls db.Database.MigrateAsync() at startup,
so schema + the admin account get created automatically the first time it runs, as long as the Postgres role in the connection string can log in (and has CREATEDB if the iborrow database doesn't exist yet).
