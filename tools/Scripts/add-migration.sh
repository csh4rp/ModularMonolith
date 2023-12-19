#!/bin/bash

connection_strings=("User ID=postgres;Password=Admin123!@#;Host=localhost;Port=5432;Database=ModularMonolithDb;")

contexts=("InternalDbContext")

projects=("../../src/Shared/ModularMonolith.Shared.Migrations")

startup_projects=("../../src/Shared/ModularMonolith.Shared.Migrations")

index=NONE

length=${#contexts[@]}

for (( i=0; i < "$length"; i++ ));
do
  cnx="${contexts[$i]}"
  
  if [[ $cnx -eq $1 ]]; then
    index=$i
  fi
done

if [[ $index == "NONE" ]]; then
  echo Context: "$1" was not found
else

  echo Running migrations for context: "${contexts[$index]}"
  
  dotnet ef migrations add \
    --context "$1" \
    --project "${projects[$index]}" \
    --startup-project "${startup_projects[$index]}" \
    "$2" \
    -- "${connection_strings[$index]}"
fi