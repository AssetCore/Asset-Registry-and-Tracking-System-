# APIGateway Service

This is an API Gateway service using Ocelot for routing and Swashbuckle for Swagger documentation.

## How to run

1. Restore dependencies:
   ```bash
   dotnet restore
   ```
2. Run the service:
   ```bash
   dotnet run
   ```
3. Access Swagger UI at:
   - http://localhost:5000/swagger
   - https://localhost:5001/swagger

## Configuration
- Logging and other settings are in `appsettings.json`.
