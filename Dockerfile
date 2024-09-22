FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet restore

FROM build AS publish-rest
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet publish "src/Startup/ModularMonolith.Startup.RestApi/ModularMonolith.Startup.RestApi.csproj" \
  -c Release -o /artifacts --no-restore

FROM build AS publish-background-services
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet publish "src/Startup/ModularMonolith.Startup.BackgroundServices/ModularMonolith.Startup.BackgroundServices.csproj" \
  -c Release -o /artifacts --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final-rest
WORKDIR /app
COPY --from=publish-rest /artifacts ./
ARG UID=10001
RUN adduser \
  --disabled-password \
  --gecos "" \
  --home "/nonexistent" \
  --shell "/sbin/nologin" \
  --no-create-home \
  --uid "${UID}" \
  appuser

USER appuser
HEALTHCHECK NONE
ENTRYPOINT ["dotnet", "ModularMonolith.Startup.RestApi.csproj.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final-background-services
WORKDIR /app
COPY --from=publish-background-services /artifacts ./
ARG UID=10001
RUN adduser \
  --disabled-password \
  --gecos "" \
  --home "/nonexistent" \
  --shell "/sbin/nologin" \
  --no-create-home \
  --uid "${UID}" \
  appuser

USER appuser
HEALTHCHECK NONE
ENTRYPOINT ["dotnet", "ModularMonolith.Startup.BackgroundServices.csproj.dll"]
