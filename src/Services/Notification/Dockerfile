# ===========================================
# BUILD STAGE
# ===========================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy entire repo into the image
COPY . .

# Restore NuGet packages for Notification API
RUN dotnet restore "src/Services/Notification/Notification.Api/Notification.Api.csproj"

# Publish the Notification API
RUN dotnet publish "src/Services/Notification/Notification.Api/Notification.Api.csproj" \
    -c Release -o /app/publish

# =========================================
# RUNTIME STAGE
# =========================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Listen on port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Notification.Api.dll"]
