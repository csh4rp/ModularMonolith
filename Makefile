migration-name := 'initial'

start-infrastructure:
	docker-compose -f tools/Docker/docker-compose.yml up -d

add-migration:
	dotnet ef migrations add --context ApplicationDbContext \
	--project src/Infrastructure/ModularMonolith.Infrastructure.Migrations \
	--startup-project src/Infrastructure/ModularMonolith.Infrastructure.Migrations \
	$(migration-name) \
	-- "User ID=postgres;Password=Admin123!@#;Host=localhost;Port=5432;Database=modular_monolith;"
	
update-database:
	dotnet ef database update --context ApplicationDbContext \
	--startup-project src/Infrastructure/ModularMonolith.Infrastructure.Migrations \
	-- "User ID=postgres;Password=Admin123!@#;Host=localhost;Port=5432;Database=modular_monolith;"

create-database:
	$(eval database_image_id := $(shell docker ps --filter name=modular_monolith_db -aq))
	docker exec ${database_image_id} psql -U postgres -c "CREATE DATABASE modular_monolith;"
	

seed-database:
	$(eval seed_script := $(shell cat tools/Scripts/add-user.sql))
	$(eval database_image_id := $(shell docker ps --filter name=modular_monolith_db -aq))
	docker cp tools/Scripts/seed-database.sql ${database_image_id}:/tmp/seed-database.sql
	docker exec ${database_image_id} psql -U postgres -d modular_monolith -f /tmp/add-user.sql