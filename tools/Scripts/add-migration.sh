#!/bin/bash

contexts=("ApplicationDbContext")

connection_strings=("User ID=postgres;Password=Admin123!@#;Host=localhost;Port=5432;Database=modular_monolith;")

projects=("../../src/Bootstrapper/ModularMonolith.Bootstrapper.Migrations" )

index=-1

length=${#contexts[@]}

for (( i=0; i < "$length"; i++ ));
do
  cnx="${contexts[$i]}"
  
  echo "$cnx"
  
  if [[ "$1" == "$cnx" ]]; then
    index=$i
    break;
  fi
done

if [[ $index == -1 ]]; then
  echo Context: "$1" was not found
else

  echo Adding migration: "$2" for context: "${contexts[$index]}"
  
  echo Project: "${projects[$index]}"
  

  dotnet ef migrations add \
    --context "$1" \
    --project "${projects[$index]}" \
    --startup-project "${projects[$index]}" \
    "$2" \
    -- "${connection_strings[$index]}"
fi