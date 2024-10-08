migration-name := 'Initial'
connection-string := 'User ID=postgres;Password=Admin123!@\#;Host=localhost;Port=5432;Database=modular_monolith'

start-infrastructure:
	docker-compose -f docker-compose.yml up -d

add-migration:
	dotnet ef migrations add --context PostgresDbContext \
	--project src/Infrastructure/Postgres/ModularMonolith.Infrastructure.Migrations.Postgres \
	--startup-project src/Infrastructure/Postgres/ModularMonolith.Infrastructure.Migrations.Postgres \
	$(migration-name) \
	-- ${connection-string}

update-database:
	dotnet ef database update --context PostgresDbContext \
	--startup-project src/Infrastructure/Postgres/ModularMonolith.Infrastructure.Migrations.Postgres \
	-- ${connection-string}

create-database:
	$(eval database_image_id := $(shell docker ps --filter name=modular_monolith_db -aq))
	docker exec ${database_image_id} psql -U postgres -c "CREATE DATABASE modular_monolith;"

database_container_name := "modular_monolith_db"
seed-database:
	$(eval seed_script := $(shell cat tools/Scripts/add-user.sql))
	docker cp tools/Scripts/seed-database.sql ${database_container_name}:/tmp/seed-database.sql
	docker exec ${database_container_name} psql -U postgres -d modular_monolith -f /tmp/seed-database.sql
