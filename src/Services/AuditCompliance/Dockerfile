# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["src/Shared/SharedKernel/SharedKernel.csproj", "Shared/SharedKernel/"]
COPY ["src/Shared/BuildingBlocks/BuildingBlocks.csproj", "Shared/BuildingBlocks/"]

# Copy csproj files and restore dependencies
COPY ["src/Services/AuditCompliance/AuditCompliance.Api/AuditCompliance.Api.csproj", "AuditCompliance.Api/"]
COPY ["src/Services/AuditCompliance/AuditCompliance.Application/AuditCompliance.Application.csproj", "AuditCompliance.Application/"]
COPY ["src/Services/AuditCompliance/AuditCompliance.Domain/AuditCompliance.Domain.csproj", "AuditCompliance.Domain/"]
COPY ["src/Services/AuditCompliance/AuditCompliance.Infrastructure/AuditCompliance.Infrastructure.csproj", "AuditCompliance.Infrastructure/"]

RUN dotnet restore "AuditCompliance.Api/AuditCompliance.Api.csproj"

# Copy everything else and build
COPY src/Shared/SharedKernel/ Shared/SharedKernel/
COPY src/Shared/BuildingBlocks/ Shared/BuildingBlocks/
COPY src/Services/AuditCompliance/ .
WORKDIR "/src/AuditCompliance.Api"
RUN dotnet build "AuditCompliance.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AuditCompliance.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuditCompliance.Api.dll"]
