#!/bin/bash

contexts=("ApplicationDbContext")

connection_strings=("User ID=postgres;Password=Admin123!@#;Host=localhost;Port=5432;Database=modular_monolith;")

projects=("../../src/Bootstrapper/ModularMonolith.Bootstrapper.Migrations" )

for (( i=0; i < "${#contexts[@]}"; i++ ));
do
  
  cnx="${contexts[$i]}"

  echo Running migrations for context: "$cnx"

  dotnet ef database update \
    --context "$cnx" \
    --startup-project "${projects[$i]}" \
    -- "${connection_strings[$i]}"
    
done