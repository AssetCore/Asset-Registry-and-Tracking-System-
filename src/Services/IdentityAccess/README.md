# Asgardeo Microservice

A .NET 9 microservice that integrates with Asgardeo REST API for user management.


## Configuration

Update the `appsettings.json` file with your Asgardeo credentials:

```json
{
  "AsgardeoSettings": {
    "OrganizationName": "your-organization-name",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Scope": "internal_application_mgt_view"
  }
}
```

## Running the Application

```bash
dotnet run --urls "http://localhost:5000;https://localhost:5001"
```

## Architecture

- **Controllers**: Handle HTTP requests and responses
- **Services**: Business logic and external API communication
- **Models**: Data transfer objects
- **Configuration**: Settings and dependency injection setup

