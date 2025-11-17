# LoadBalancer.Api (Gateway)

A simple ASP.NET Core reverse proxy and load balancer for the microservices in this solution, built with YARP.

## What it does
- Exposes a single entry point and forwards requests to backend services.
- Route prefixes:
  - `/assetregistry/*` -> AssetRegistry.Api
  - `/audit/*` -> AuditCompliance.Api
  - `/identity/*` -> IdentityAccess Api
  - `/maintenance/*` -> MaintenanceScheduler.Api
  - `/notification/*` -> Notification.Api
- Basic health endpoint at `/health` and liveness at `/`.

## Prerequisites
- .NET SDK 9 (to build and run)
- Install the YARP package:

```powershell
# From repo root
dotnet add .\src\Services\Gateway\LoadBalancer.Api\LoadBalancer.Api.csproj package Yarp.ReverseProxy
```

If your solution uses a central package version file, you can also pin the package version there.

## Project layout (Clean Architecture style)
```
src/Services/Gateway/
  LoadBalancer.Api/            # ASP.NET Core host + YARP (entry point)
  LoadBalancer.Application/    # Policies, auth handlers, abstractions (optional)
  LoadBalancer.Domain/         # Domain models if any (usually empty for gateways)
  LoadBalancer.Infrastructure/ # Impl for config/service discovery/logging (optional)
```

References:
- `LoadBalancer.Api` -> `LoadBalancer.Application`, `LoadBalancer.Infrastructure`
- `LoadBalancer.Infrastructure` -> `LoadBalancer.Application`, `LoadBalancer.Domain`
- `LoadBalancer.Application` -> `LoadBalancer.Domain`

Add to solution (once .NET SDK is available):
```powershell
dotnet sln .\AssetRegistryAndTrackingSystem.sln add \
  .\src\Services\Gateway\LoadBalancer.Domain\LoadBalancer.Domain.csproj \
  .\src\Services\Gateway\LoadBalancer.Application\LoadBalancer.Application.csproj \
  .\src\Services\Gateway\LoadBalancer.Infrastructure\LoadBalancer.Infrastructure.csproj \
  .\src\Services\Gateway\LoadBalancer.Api\LoadBalancer.Api.csproj
```

## Configure backend addresses
Default destinations are set in `appsettings.json` (localhost ports). Override them via environment variables if your services run on different URLs/ports:

- `ReverseProxy__Clusters__asset-registry__Destinations__d1__Address`
- `ReverseProxy__Clusters__audit-compliance__Destinations__d1__Address`
- `ReverseProxy__Clusters__identity-access__Destinations__d1__Address`
- `ReverseProxy__Clusters__maintenance-scheduler__Destinations__d1__Address`
- `ReverseProxy__Clusters__notification__Destinations__d1__Address`

Example:
```powershell
$env:ReverseProxy__Clusters__asset-registry__Destinations__d1__Address = "http://localhost:5005/"
$env:ReverseProxy__Clusters__identity-access__Destinations__d1__Address = "http://localhost:5010/"
```

## Add to solution and run
```powershell
# From repo root
# Add the gateway projects to the solution (optional)
 dotnet sln .\AssetRegistryAndTrackingSystem.sln add \
   .\src\Services\Gateway\LoadBalancer.Domain\LoadBalancer.Domain.csproj \
   .\src\Services\Gateway\LoadBalancer.Application\LoadBalancer.Application.csproj \
   .\src\Services\Gateway\LoadBalancer.Infrastructure\LoadBalancer.Infrastructure.csproj \
   .\src\Services\Gateway\LoadBalancer.Api\LoadBalancer.Api.csproj

# IMPORTANT: If solution restore fails due to other missing projects,
# restore and run ONLY the gateway project:
 dotnet restore .\src\Services\Gateway\LoadBalancer.Api\LoadBalancer.Api.csproj
 dotnet run --project .\src\Services\Gateway\LoadBalancer.Api\LoadBalancer.Api.csproj --urls http://localhost:5000
```

### Visual Studio (alternative)
- Open `AssetRegistryAndTrackingSystem.sln`.
- Right-click the solution > Add > Existing Project... > select `LoadBalancer.Api.csproj`.
- Set multiple startup projects (gateway + target APIs) if desired.

### HTTPS dev certificate (optional)
If you use HTTPS locally, make sure the dev cert is trusted:
```powershell
dotnet dev-certs https --trust
```

## Try it
- Health: `GET http://localhost:5000/health` (or the port shown in logs)
- Example proxied call: `GET http://localhost:5000/assetregistry/swagger/index.html` (adjust ports/routes as needed)

You can also use the included `gateway.http` file for quick tests.

If port 5000 is busy or fails to bind, try a different port:
```powershell
dotnet run --project .\src\Services\Gateway\LoadBalancer.Api\LoadBalancer.Api.csproj --urls http://localhost:5050
```

## Next steps (optional)
- Add auth at the gateway (JWT validation) before proxying.
- Add rate limiting and circuit breakers.
- Configure HTTPS and production-ready logging.
- Containerize and deploy behind an external load balancer (NGINX/Azure Front Door).

## Troubleshooting
- 404 at gateway: Verify the route prefix and that the backend endpoint exists.
- 502/503: Backend service not running or wrong port; update cluster `Address` or env vars.
- CORS errors: CORS is open by default here for dev. Tighten or configure per origin if needed.
- HTTPS loops: Ensure backend `Address` matches the scheme (http vs https) the service listens on.
