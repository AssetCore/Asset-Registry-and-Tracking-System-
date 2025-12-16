# ===========================================
# BUILD STAGE
# ===========================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy entire repo into the image
COPY . .

# Restore NuGet packages for AuditCompliance API
RUN dotnet restore "src/Services/AuditCompliance/AuditCompliance.Api/AuditCompliance.Api.csproj"

# Publish the AuditCompliance API
RUN dotnet publish "src/Services/AuditCompliance/AuditCompliance.Api/AuditCompliance.Api.csproj" \
    -c Release -o /app/publish

# ==========================================
# RUNTIME STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Listen on port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 5002

ENTRYPOINT ["dotnet", "AuditCompliance.Api.dll"]
