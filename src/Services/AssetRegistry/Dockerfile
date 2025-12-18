# ===========================================
# BUILD STAGE
# ===========================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy *all* source from repo root into the image
COPY . .

# Restore for the API project
RUN dotnet restore "src/Services/AssetRegistry/AssetRegistry.Api/AssetRegistry.Api.csproj"

# Publish the API project
RUN dotnet publish "src/Services/AssetRegistry/AssetRegistry.Api/AssetRegistry.Api.csproj" \
    -c Release -o /app/publish

# =========================================
# RUNTIME STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AssetRegistry.Api.dll"]
