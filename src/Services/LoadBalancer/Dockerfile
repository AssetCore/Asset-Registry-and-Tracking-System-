# ===========================================
# BUILD STAGE
# ===========================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy *all* source from repo root into the image
COPY . .

# Restore for the API project
RUN dotnet restore "src/Services/LoadBalancer/LoadBalancer.Api/LoadBalancer.Api.csproj"

# Publish the API project with optimizations for production
RUN dotnet publish "src/Services/LoadBalancer/LoadBalancer.Api/LoadBalancer.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ===========================================
# RUNTIME STAGE
# ===========================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks (useful for container orchestrators)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Environment variables for production
ENV ASPNETCORE_URLS=http://+:8080;https://+:8443 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certs/aspnetapp.pfx \
    ASPNETCORE_Kestrel__Certificates__Default__Password="" \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

# Expose ports 8080 (HTTP) and 8443 (HTTPS)
EXPOSE 8080 8443

# Health check configuration
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "LoadBalancer.Api.dll"]
