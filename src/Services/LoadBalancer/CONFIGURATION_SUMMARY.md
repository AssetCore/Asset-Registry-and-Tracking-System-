# LoadBalancer Service - Public to Private Subnet Configuration Summary

## ✅ Changes Made

### 1. Updated Application Configuration

**File: `appsettings.json`**
- ✅ Changed all backend destinations from `localhost:PORT` to Istio ingress gateway
- ✅ Added active health checks for all clusters (30s interval)
- ✅ Configured HTTP request timeouts (60s)
- ✅ Added request header forwarding (`X-Forwarded-Host`)
- ✅ Routes now preserve original paths and forward to Istio

**Backend Address (Default):**
```
http://istio-ingressgateway.istio-system.svc.cluster.local
```

**Health Check:**
```
http://istio-ingressgateway.istio-system.svc.cluster.local:15021/healthz/ready
```

### 2. Enhanced Application Code

**File: `Program.cs`**
- ✅ Added structured JSON logging for production
- ✅ Configured HTTP client with connection pooling (10min lifetime, 5min idle)
- ✅ Enhanced forwarded headers (XForwardedFor, XForwardedProto, XForwardedHost)
- ✅ **Removed HTTPS redirection** (TLS termination at external LB)
- ✅ Added custom YARP health check implementation
- ✅ Added liveness (`/health/live`) and readiness (`/health/ready`) endpoints
- ✅ Added request logging middleware for debugging

### 3. Production-Ready Dockerfile

**File: `Dockerfile`**
- ✅ Multi-stage build with optimizations
- ✅ Non-root user (appuser:1000) for security
- ✅ Health check configuration (curl-based)
- ✅ Installed curl for container health checks
- ✅ Production environment variables
- ✅ Proper file ownership and permissions

### 4. Deployment Configurations

**Created Files:**
- ✅ `DEPLOYMENT_GUIDE.md` - Comprehensive deployment documentation
- ✅ `appsettings.Production.json` - Production configuration template
- ✅ `k8s-public-deployment.yaml` - Kubernetes deployment with internet-facing LB
- ✅ `aws-ecs-task-definition.json` - AWS ECS Fargate task definition

**Updated Files:**
- ✅ `README.md` - Updated with public subnet deployment instructions

## 📊 Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                           INTERNET                               │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ HTTPS (443)
                             │
┌────────────────────────────▼────────────────────────────────────┐
│                      PUBLIC SUBNET                               │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  AWS ALB/NLB (Internet-facing)                          │   │
│  │  - TLS Termination                                       │   │
│  │  - Health Checks: /health/ready                         │   │
│  └───────────────────────┬─────────────────────────────────┘   │
│                          │ HTTP (8080)                           │
│  ┌───────────────────────▼─────────────────────────────────┐   │
│  │  LoadBalancer.Api (YARP) - THIS SERVICE                 │   │
│  │  - Receives public traffic                              │   │
│  │  - Rate limiting (100 req/min)                          │   │
│  │  - Request forwarding with headers                      │   │
│  │  - Active health checks to backend                      │   │
│  └───────────────────────┬─────────────────────────────────┘   │
└────────────────────────────┼────────────────────────────────────┘
                             │ HTTP (80/443)
                             │ Health: Port 15021
                             │
┌────────────────────────────▼────────────────────────────────────┐
│                     PRIVATE SUBNET                               │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Internal NLB (for Istio Gateway)                       │   │
│  │  - scheme: internal                                      │   │
│  └───────────────────────┬─────────────────────────────────┘   │
│                          │                                       │
│  ┌───────────────────────▼─────────────────────────────────┐   │
│  │  Istio Ingress Gateway (istio-system namespace)         │   │
│  │  - Port 80/443 (traffic)                                │   │
│  │  - Port 15021 (health checks)                           │   │
│  │  - Routes to backend services via VirtualService        │   │
│  └───────────────────────┬─────────────────────────────────┘   │
│                          │                                       │
│  ┌───────────────────────▼─────────────────────────────────┐   │
│  │  Kubernetes Services (ClusterIP)                        │   │
│  │  ┌─────────────────┐  ┌──────────────────┐             │   │
│  │  │ AssetRegistry   │  │ IdentityAccess   │  ...        │   │
│  │  │ Pods            │  │ Pods             │             │   │
│  │  └─────────────────┘  └──────────────────┘             │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## 🔑 Key Configuration Points

### 1. Route Configuration

All routes now use `PathPattern` transform to preserve the original path:

```json
{
  "ClusterId": "asset-registry",
  "Match": { "Path": "/assetregistry/{**catch-all}" },
  "Transforms": [ 
    { "PathPattern": "/assetregistry/{**catch-all}" },
    { "RequestHeader": "X-Forwarded-Host", "Set": "{header:host}" }
  ]
}
```

**Before:** `/assetregistry/api/assets` → Backend: `/api/assets` (prefix removed)  
**After:** `/assetregistry/api/assets` → Backend: `/assetregistry/api/assets` (full path preserved)

This is critical because Istio VirtualService needs the full path to route correctly.

### 2. Health Checks

**Active Health Checks:**
- **Enabled:** Yes
- **Interval:** 30 seconds
- **Timeout:** 10 seconds
- **Policy:** ConsecutiveFailures
- **Path:** `/healthz/ready` (Istio standard health endpoint)

**LoadBalancer Health Endpoints:**
- `/health/live` - Container liveness (always returns 200 if app is running)
- `/health/ready` - Readiness probe (includes YARP health checks)
- `/health` - Legacy compatibility endpoint

### 3. Connection Pooling

Optimized for long-lived connections to private subnet:

```csharp
handler.PooledConnectionLifetime = TimeSpan.FromMinutes(10);
handler.PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5);
handler.MaxConnectionsPerServer = 100;
```

### 4. Rate Limiting

Global rate limiter to protect backend:
- **Limit:** 100 requests per minute
- **Queue:** Disabled (immediate rejection on limit)
- **Response:** 429 Too Many Requests

### 5. Security Headers

Forwarded headers preserve client information through the proxy chain:
- `X-Forwarded-For` - Original client IP
- `X-Forwarded-Proto` - Original protocol (http/https)
- `X-Forwarded-Host` - Original host header

## 🚀 Deployment Options

### Option 1: AWS ECS (Recommended)

**Advantages:**
- Fully managed container orchestration
- Easy to deploy in public subnets with VPC connectivity
- Integrates with AWS ALB/NLB
- Auto-scaling with CPU/memory metrics
- CloudWatch logging out-of-the-box

**Steps:**
1. Push image to ECR
2. Create ECS cluster in public subnets
3. Register task definition (`aws-ecs-task-definition.json`)
4. Create ECS service with ALB
5. Configure target group health checks on `/health/ready`

### Option 2: Kubernetes (Public Cluster)

**Advantages:**
- Consistent with existing Kubernetes infrastructure
- Native HPA support
- Pod disruption budgets
- Service mesh integration possible

**Steps:**
1. Push image to container registry (GHCR/ECR)
2. Update `k8s-public-deployment.yaml` with correct Istio gateway address
3. Apply: `kubectl apply -f k8s-public-deployment.yaml`
4. Service creates internet-facing LoadBalancer automatically

### Option 3: Docker Standalone (Testing/Development)

**Advantages:**
- Simple for testing
- No orchestration required
- Quick iteration

**Steps:**
1. Build: `docker build -t loadbalancer:latest -f src/Services/LoadBalancer/Dockerfile .`
2. Run with environment variables
3. Use Docker networking or host network mode

## 🔒 Security Considerations

### Network Security

1. **Security Groups (AWS):**
   - Public LB SG: Allow 80/443 from 0.0.0.0/0, allow 8080 from ALB
   - Private Istio SG: Allow 80/443/15021 from Public LB SG only

2. **VPC Configuration:**
   - Public subnet: IGW route for 0.0.0.0/0
   - Private subnet: NAT Gateway for outbound only
   - VPC peering if different VPCs

3. **Service Isolation:**
   - LoadBalancer cannot directly access Kubernetes pods
   - All traffic goes through Istio gateway
   - Istio enforces mTLS between services

### Application Security

1. **Non-Root Container:** Runs as user 1000 (appuser)
2. **No HTTPS Redirection:** TLS terminated at ALB
3. **Rate Limiting:** Global rate limiter enabled
4. **CORS:** Currently allows all origins (configure for production)
5. **Authentication:** Placeholder for JWT validation (configure as needed)

## 📝 Environment Variables Reference

### Required (Production)

```bash
# Backend Istio Gateway
ReverseProxy__Clusters__asset-registry__Destinations__istio-gateway__Address=http://internal-istio-nlb.elb.amazonaws.com

# Apply same to all clusters:
# - asset-registry
# - audit-compliance
# - identity-access
# - maintenance-scheduler
# - notification
# - apigateway
```

### Optional

```bash
# Logging
Logging__LogLevel__Default=Information
Logging__LogLevel__Yarp=Information
Logging__LogLevel__Microsoft.AspNetCore=Warning

# Rate Limiting
RateLimiter__PermitLimit=100
RateLimiter__Window=00:01:00

# Authentication (if enabled)
Authentication__Authority=https://your-identity-provider
Authentication__Audience=your-api-audience

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

## 🧪 Testing

### Local Testing (with kubectl port-forward)

```bash
# Terminal 1: Port forward Istio gateway
kubectl port-forward -n istio-system svc/istio-ingressgateway 8080:80

# Terminal 2: Run LoadBalancer
cd src/Services/LoadBalancer/LoadBalancer.Api
dotnet run --urls http://localhost:5000

# Terminal 3: Test
curl http://localhost:5000/health/ready
curl http://localhost:5000/assetregistry/health
```

### Integration Testing (Deployed)

```bash
# Health check
curl -i http://your-loadbalancer-dns/health/ready

# Route test
curl -i http://your-loadbalancer-dns/assetregistry/api/assets

# Rate limit test
for i in {1..150}; do curl http://your-loadbalancer-dns/; done
```

## 📊 Monitoring

### Key Metrics

1. **Latency:** Time from LoadBalancer to Istio gateway
2. **Error Rate:** 5xx responses from backend
3. **Health Check Success Rate:** `/healthz/ready` checks
4. **Rate Limit Hits:** 429 response count
5. **Connection Pool:** Active connections

### CloudWatch Log Insights Queries

```
# Error logs
fields @timestamp, @message
| filter @message like /ERROR/
| sort @timestamp desc

# Slow requests
fields @timestamp, Method, Path, Duration
| filter Duration > 1000
| sort Duration desc

# Rate limit hits
fields @timestamp, @message
| filter @message like /429/
| stats count() by bin(5m)
```

## 🎯 Next Steps

1. **Deploy to Public Subnet:**
   - Choose deployment method (ECS/Kubernetes/Docker)
   - Update Istio gateway address in configuration
   - Configure security groups
   - Deploy and test

2. **Configure DNS:**
   - Point your domain to public LoadBalancer
   - Configure SSL/TLS certificate at ALB/NLB
   - Update CORS policy with actual domain

3. **Enable Monitoring:**
   - Set up CloudWatch alarms
   - Configure log aggregation
   - Create dashboard for key metrics

4. **Security Hardening:**
   - Restrict CORS to specific origins
   - Enable JWT authentication if needed
   - Implement API key validation
   - Configure WAF rules at ALB

5. **Performance Tuning:**
   - Adjust rate limits based on traffic
   - Tune connection pool settings
   - Enable HPA with appropriate thresholds
   - Load test and optimize

## 📚 References

- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [Istio Gateway Configuration](https://istio.io/latest/docs/reference/config/networking/gateway/)
- [AWS ECS Networking](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-networking.html)
- [Kubernetes Services](https://kubernetes.io/docs/concepts/services-networking/service/)
