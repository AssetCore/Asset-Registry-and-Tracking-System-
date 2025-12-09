# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["AuditCompliance.Api/AuditCompliance.Api.csproj", "AuditCompliance.Api/"]
COPY ["AuditCompliance.Application/AuditCompliance.Application.csproj", "AuditCompliance.Application/"]
COPY ["AuditCompliance.Domain/AuditCompliance.Domain.csproj", "AuditCompliance.Domain/"]
COPY ["AuditCompliance.Infrastructure/AuditCompliance.Infrastructure.csproj", "AuditCompliance.Infrastructure/"]

RUN dotnet restore "AuditCompliance.Api/AuditCompliance.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/AuditCompliance.Api"
RUN dotnet build "AuditCompliance.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AuditCompliance.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuditCompliance.Api.dll"]
