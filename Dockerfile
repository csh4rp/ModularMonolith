FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages dotnet build

FROM build AS test
RUN dotnet test ./app/ModularMonolith.sln --no-build --no-restore --filter "FullyQualifiedName~.ArchitectureTests"
RUN dotnet test --no-restore --no-build --logger trx --results-directory "test-results" --collect:"XPlat Code Coverage" --filter "FullyQualifiedName~.UnitTests"

FROM build AS publish
COPY ./src ./
RUN dotnet publish "./Startup/ModularMonolith.Startup.RestApi/ModularMonolith.Startup.RestApi.csproj" -c release -o /app --no-restore


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app
COPY --from=publish /app ./
ENTRYPOINT ["dotnet", "ModularMonolith.Startup.RestApi.csproj.dll"]
