#!/bin/bash

contexts=("InternalDbContext" "FirstModuleDbContext")

connection_strings=("User ID=postgres;Password=Admin123!@#;Host=localhost;Port=5432;Database=modular_monolith;" \
  "User ID=postgres;Password=Admin123!@#;Host=localhost;Port=5432;Database=modular_monolith;")

projects=("../../src/Shared/ModularMonolith.Shared.Migrations" \
  "../../src/Modules/FirstModule/ModularMonolith.Modules.FirstModule.Migrations")

index=-1

length=${#contexts[@]}

for (( i=0; i < "$length"; i++ ));
do
  cnx="${contexts[$i]}"
  
  if [[ $cnx -eq $1 ]]; then
    index=$i
  fi
done

if [[ $index == -1 ]]; then
  echo Context: "$1" was not found
else

  echo Addding migration: "$2" for context: "${contexts[$index]}"
  
  dotnet ef migrations add \
    --context "$1" \
    --project "${projects[$index]}" \
    --startup-project "${projects[$index]}" \
    "$2" \
    -- "${connection_strings[$index]}"
fi