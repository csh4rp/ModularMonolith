FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet restore

FROM build AS publish
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet publish "src/Startup/ModularMonolith.Startup.RestApi/ModularMonolith.Startup.RestApi.csproj" \
  -c release -o /artifacts --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /artifacts ./
ARG UID=10001
RUN adduser \
  --disabled-password \
  --gecos "" \
  --home "/nonexistent" \
  -- shell "/sbin/nologin" \
  --no-create-home \
  --uid "${UID}" \
  appuser
USER appuser
ENTRYPOINT ["dotnet", "ModularMonolith.Startup.RestApi.csproj.dll"]
