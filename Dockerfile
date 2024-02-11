# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.props ./

# copy csproj and restore as distinct layers
COPY src/Startup/ModularMonolith.Startup.RestApi/*.csproj ./Startup/ModularMonolith.Startup.RestApi/

COPY src/Infrastructure/ModularMonolith.Infrastructure.DataAccess/*.csproj ./Infrastructure/ModularMonolith.Infrastructure.DataAccess/

COPY src/Modules/CategoryManagement/ModularMonolith.CategoryManagement.Api/*.csproj ./Modules/CategoryManagement/ModularMonolith.CategoryManagement.Api/
COPY src/Modules/CategoryManagement/ModularMonolith.CategoryManagement.Domain/*.csproj ./Modules/CategoryManagement/ModularMonolith.CategoryManagement.Domain/
COPY src/Modules/CategoryManagement/ModularMonolith.CategoryManagement.Application/*.csproj ./Modules/CategoryManagement/ModularMonolith.CategoryManagement.Application/
COPY src/Modules/CategoryManagement/ModularMonolith.CategoryManagement.Infrastructure/*.csproj ./Modules/CategoryManagement/ModularMonolith.CategoryManagement.Infrastructure/
COPY src/Modules/CategoryManagement/ModularMonolith.CategoryManagement.Contracts/*.csproj ./Modules/CategoryManagement/ModularMonolith.CategoryManagement.Contracts/

COPY src/Modules/Identity/ModularMonolith.Identity.Api/*.csproj ./Modules/Identity/ModularMonolith.Identity.Api/
COPY src/Modules/Identity/ModularMonolith.Identity.Domain/*.csproj ./Modules/Identity/ModularMonolith.Identity.Domain/
COPY src/Modules/Identity/ModularMonolith.Identity.Application/*.csproj ./Modules/Identity/ModularMonolith.Identity.Application/
COPY src/Modules/Identity/ModularMonolith.Identity.Infrastructure/*.csproj ./Modules/Identity/ModularMonolith.Identity.Infrastructure/
COPY src/Modules/Identity/ModularMonolith.Identity.Contracts/*.csproj ./Modules/Identity/ModularMonolith.Identity.Contracts/
COPY src/Modules/Identity/ModularMonolith.Identity.Core/*.csproj ./Modules/Identity/ModularMonolith.Identity.Core/

COPY src/Shared/ModularMonolith.Shared.Api/*.csproj ./Shared/ModularMonolith.Shared.Api/
COPY src/Shared/ModularMonolith.Shared.Application/*.csproj ./Shared/ModularMonolith.Shared.Application/
COPY src/Shared/ModularMonolith.Shared.Domain/*.csproj ./Shared/ModularMonolith.Shared.Domain/
COPY src/Shared/ModularMonolith.Shared.Contracts/*.csproj ./Shared/ModularMonolith.Shared.Contracts/
COPY src/Shared/ModularMonolith.Shared.Infrastructure/*.csproj ./Shared/ModularMonolith.Shared.Infrastructure/

RUN dotnet restore "./Startup/ModularMonolith.Startup.RestApi/ModularMonolith.Startup.RestApi.csproj"

# copy everything else and build app
COPY ./src ./
RUN ls ./Startup/ModularMonolith.Startup.RestApi/
RUN dotnet publish "./Startup/ModularMonolith.Startup.RestApi/ModularMonolith.Startup.RestApi.csproj" -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "ModularMonolith.Startup.RestApi.csproj.dll"]
