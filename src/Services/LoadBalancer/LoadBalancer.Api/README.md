# LoadBalancer.Api (Public Gateway)

A production-ready ASP.NET Core reverse proxy and load balancer built with YARP, designed to run in a **public subnet** and route traffic to your **private Kubernetes/Istio infrastructure**.

## 🎯 Purpose

This service acts as a public-facing entry point that:
- Accepts internet traffic from a public subnet
- Routes requests to an internal Istio ingress gateway in a private subnet
- Provides health checking, rate limiting, and request forwarding
- Maintains connection pooling for optimal performance

## 📋 What it does

**Architecture Flow:**
```
Internet → Public Subnet (LoadBalancer.Api) → Private Subnet (Istio Gateway) → Kubernetes Pods
```

**Route prefixes:**
- `/assetregistry/*` → AssetRegistry.Api (via Istio)
- `/audit/*` → AuditCompliance.Api (via Istio)
- `/identity/*` → IdentityAccess.Api (via Istio)
- `/maintenance/*` → MaintenanceScheduler.Api (via Istio)
- `/notification/*` → Notification.Api (via Istio)
- `/api/*` → APIGateway (via Istio)

**Health endpoints:**
- `/health/live` - Liveness probe (200 OK if service is alive)
- `/health/ready` - Readiness probe (200 OK if backends are healthy)
- `/health` - Legacy health check
- `/` - Service information JSON

## Prerequisites
- .NET SDK 9 (to build and run)
- Docker (for containerization)
- Access to your private Istio ingress gateway
- Network connectivity between public and private subnets

## Quick Start (Development)

### Local Development with Port Forwarding

Test locally by port-forwarding to your Istio gateway:

```powershell
# Port forward to Istio gateway in your private cluster
kubectl port-forward -n istio-system svc/istio-ingressgateway 8080:80

# Run LoadBalancer (it will connect to localhost:8080)
dotnet run --project .\src\Services\LoadBalancer\LoadBalancer.Api\LoadBalancer.Api.csproj --urls http://localhost:5000

# Test
curl http://localhost:5000/health/ready
curl http://localhost:5000/assetregistry/health
```

## Production Deployment

See [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) for comprehensive deployment instructions including:
- AWS ECS deployment (recommended for public subnet)
- Kubernetes deployment with internet-facing LoadBalancer
- Docker standalone deployment
- Networking and security group configuration

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

## Configuration

### Backend Routing (Production)

**Default Configuration** (`appsettings.json`):
- Routes all traffic through Istio Ingress Gateway
- Uses Kubernetes internal DNS: `istio-ingressgateway.istio-system.svc.cluster.local`
- Active health checks every 30 seconds

**Override for AWS/Cloud** (`appsettings.Production.json` or environment variables):
```bash
# Use internal NLB DNS name for Istio gateway
export ReverseProxy__Clusters__asset-registry__Destinations__istio-gateway__Address="http://internal-istio-nlb-xxxxx.elb.us-east-1.amazonaws.com"
```

### Environment Variables

Key configuration overrides:

```bash
# Backend Istio Gateway Address
ReverseProxy__Clusters__asset-registry__Destinations__istio-gateway__Address=http://your-istio-gateway

# Rate Limiting
RateLimiter__PermitLimit=100
RateLimiter__Window=00:01:00

# Logging
Logging__LogLevel__Default=Information
Logging__LogLevel__Yarp=Information

# Authentication (optional)
Authentication__Authority=https://your-identity-provider
Authentication__Audience=your-api-audience
```

## Docker Build and Run

### Build Container

```powershell
# From repository root
cd d:\Github_repos\Asset-Registry-and-Tracking-System-

docker build -t loadbalancer:latest -f src/Services/LoadBalancer/Dockerfile .
```

### Run Container (Standalone)

```powershell
# Run with default configuration (for testing with port-forwarded Istio)
docker run -d \
  -p 8080:8080 \
  --name loadbalancer \
  -e ASPNETCORE_ENVIRONMENT=Development \
  loadbalancer:latest

# Run for production with custom Istio gateway
docker run -d \
  -p 8080:8080 \
  --name loadbalancer \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ReverseProxy__Clusters__asset-registry__Destinations__istio-gateway__Address=http://your-istio-nlb.amazonaws.com \
  loadbalancer:latest

# Check health
curl http://localhost:8080/health/ready
```

## Deployment Files

The repository includes production-ready deployment configurations:

| File | Purpose |
|------|---------|
| [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) | Comprehensive deployment guide |
| [k8s-public-deployment.yaml](../k8s-public-deployment.yaml) | Kubernetes deployment with internet-facing LB |
| [aws-ecs-task-definition.json](../aws-ecs-task-definition.json) | AWS ECS Fargate task definition |
| [appsettings.Production.json](appsettings.Production.json) | Production configuration template |
| [Dockerfile](../Dockerfile) | Multi-stage production Dockerfile |

## Networking Requirements

### Security Groups (AWS Example)

**Public LoadBalancer Security Group:**
```
Inbound:
  - Port 80/443 from 0.0.0.0/0 (Internet)
  - Port 8080 from ALB/NLB

Outbound:
  - Port 80/443 to Private Istio Gateway SG
  - Port 15021 to Private Istio Gateway (health checks)
```

**Private Istio Gateway Security Group:**
```
Inbound:
  - Port 80/443 from Public LoadBalancer SG
  - Port 15021 from Public LoadBalancer (health checks)
```

## Features

✅ **Active Health Checks**: Monitors Istio gateway health every 30 seconds  
✅ **Connection Pooling**: Optimized for long-lived connections  
✅ **Rate Limiting**: Global rate limiting (100 req/min default)  
✅ **Request Forwarding**: Preserves client IPs and headers  
✅ **Structured Logging**: JSON logging for CloudWatch/ELK  
✅ **Security**: Non-root container, minimal attack surface  
✅ **Kubernetes Ready**: Health probes, graceful shutdown  
✅ **Auto-scaling**: HPA configuration included  

## Monitoring

### Key Metrics to Monitor

- **Istio Gateway Health**: Check `/health/ready` endpoint
- **Response Times**: Monitor proxy latency
- **Error Rates**: Track 5xx errors from backend
- **Connection Pool**: Monitor active connections
- **Rate Limit**: Track rate limit rejections (429 responses)

### CloudWatch Alarms (AWS)

```bash
# High error rate
aws cloudwatch put-metric-alarm \
  --alarm-name loadbalancer-high-error-rate \
  --metric-name HTTPCode_Target_5XX_Count \
  --namespace AWS/ApplicationELB \
  --statistic Sum \
  --period 300 \
  --threshold 10 \
  --comparison-operator GreaterThanThreshold

# Unhealthy targets
aws cloudwatch put-metric-alarm \
  --alarm-name loadbalancer-unhealthy-targets \
  --metric-name UnHealthyHostCount \
  --namespace AWS/ApplicationELB \
  --statistic Average \
  --period 60 \
  --threshold 1 \
  --comparison-operator GreaterThanThreshold
```

## Troubleshooting

### Cannot connect to Istio gateway

**Check DNS resolution:**
```bash
# Inside container
nslookup istio-ingressgateway.istio-system.svc.cluster.local

# Or for AWS internal NLB
nslookup internal-istio-nlb-xxxxx.elb.us-east-1.amazonaws.com
```

**Verify connectivity:**
```bash
# Test from LoadBalancer to Istio
curl -v http://your-istio-gateway/healthz/ready
```

**Check security groups** allow traffic on ports 80, 443, and 15021

### High latency

- Check network latency between subnets
- Increase connection pool size
- Add more LoadBalancer replicas
- Enable connection keep-alive

### Rate limiting issues

Adjust rate limits in configuration:
```json
"RateLimiter": {
  "PermitLimit": 500,  // Increase from 100
  "Window": "00:01:00"
}
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
