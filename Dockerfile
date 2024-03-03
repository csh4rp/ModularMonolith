FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet restore

FROM build AS test
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet build
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet test --no-restore --no-build --filter "FullyQualifiedName~.ArchitectureTests"
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet test --no-restore --no-build --logger trx --results-directory "test-results" --collect:"XPlat Code Coverage" --filter "FullyQualifiedName~.UnitTests"

FROM build AS publish
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet publish "src/Startup/ModularMonolith.Startup.RestApi/ModularMonolith.Startup.RestApi.csproj" -c release -o /artifacts --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /artifacts ./
ENTRYPOINT ["dotnet", "ModularMonolith.Startup.RestApi.csproj.dll"]
