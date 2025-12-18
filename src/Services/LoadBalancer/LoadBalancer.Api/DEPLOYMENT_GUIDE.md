# LoadBalancer.Api - Public Subnet Deployment Guide

## Overview
This LoadBalancer service is designed to run in a **public subnet** and route traffic to your **private Kubernetes/Istio infrastructure**. It acts as a public-facing entry point that forwards requests to the internal Istio ingress gateway.

## Architecture

```
Internet
    ↓
[Public Subnet]
    ↓
LoadBalancer.Api (YARP) - This Service
    ↓
[Private Subnet]
    ↓
Istio Ingress Gateway
    ↓
Kubernetes Services (AssetRegistry, Identity, etc.)
```

## Key Features

✅ **Public-to-Private Routing**: Routes external traffic to private Kubernetes pods via Istio gateway  
✅ **Health Checks**: Active health monitoring of backend Istio gateway  
✅ **Connection Pooling**: Optimized for long-lived connections to private subnet  
✅ **Rate Limiting**: Global rate limiting to protect backend services  
✅ **Request Tracing**: Preserves client IPs and headers through forwarding chain  
✅ **Security**: Runs as non-root user, minimal attack surface  

## Configuration

### Backend Routing
The service routes all traffic through the **Istio Ingress Gateway** in your private subnet:

- **Default Address**: `http://istio-ingressgateway.istio-system.svc.cluster.local`
- **Health Check**: Active probing every 30 seconds on port 15021

### Supported Routes

| Path Prefix | Target Service | Description |
|------------|---------------|-------------|
| `/assetregistry/*` | AssetRegistry.Api | Asset management operations |
| `/audit/*` | AuditCompliance.Api | Audit and compliance logs |
| `/identity/*` | IdentityAccess.Api | Authentication and authorization |
| `/maintenance/*` | MaintenanceScheduler.Api | Maintenance scheduling |
| `/notification/*` | Notification.Api | Notification service |
| `/api/*` | APIGateway | Main API gateway |

### Environment Variables

Override configuration via environment variables:

```bash
# Istio Gateway Address (if different from default)
ReverseProxy__Clusters__asset-registry__Destinations__istio-gateway__Address=http://your-istio-gateway:80

# Rate Limiting
RateLimiter__PermitLimit=100
RateLimiter__Window=00:01:00

# Authentication (optional)
Authentication__Authority=https://your-identity-provider
Authentication__Audience=your-api-audience

# Logging
Logging__LogLevel__Default=Information
Logging__LogLevel__Yarp=Information
```

## Deployment Options

### Option 1: Docker Container (Standalone)

Build and run the container:

```bash
# Build
docker build -t loadbalancer:latest -f src/Services/LoadBalancer/Dockerfile .

# Run with environment variables
docker run -d \
  -p 8080:8080 \
  --name loadbalancer \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ReverseProxy__Clusters__asset-registry__Destinations__istio-gateway__Address=http://your-istio-gateway \
  loadbalancer:latest

# Check health
curl http://localhost:8080/health/ready
```

### Option 2: AWS ECS (Recommended for Public Subnet)

Deploy to AWS ECS in a public subnet with proper networking:

**Task Definition** (`loadbalancer-task.json`):
```json
{
  "family": "loadbalancer",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "containerDefinitions": [
    {
      "name": "loadbalancer",
      "image": "your-ecr-repo/loadbalancer:latest",
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "healthCheck": {
        "command": ["CMD-SHELL", "curl -f http://localhost:8080/health/live || exit 1"],
        "interval": 30,
        "timeout": 10,
        "retries": 3,
        "startPeriod": 30
      },
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        },
        {
          "name": "ReverseProxy__Clusters__asset-registry__Destinations__istio-gateway__Address",
          "value": "http://istio-nlb-internal.your-domain.com"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/loadbalancer",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

**Service Configuration**:
- **Subnets**: Public subnets with IGW route
- **Security Group**: Allow inbound on port 8080 from ALB
- **Load Balancer**: Application Load Balancer (ALB) in public subnets
- **Target Group**: Health check on `/health/ready`

### Option 3: Kubernetes (Public Cluster)

Deploy to a Kubernetes cluster in public subnet:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: public-loadbalancer
  namespace: default
spec:
  replicas: 2
  selector:
    matchLabels:
      app: public-loadbalancer
  template:
    metadata:
      labels:
        app: public-loadbalancer
    spec:
      containers:
      - name: loadbalancer
        image: your-registry/loadbalancer:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ReverseProxy__Clusters__asset-registry__Destinations__istio-gateway__Address
          value: "http://istio-ingressgateway.istio-system.svc.cluster.local"
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
        resources:
          requests:
            cpu: 250m
            memory: 512Mi
          limits:
            cpu: 1000m
            memory: 1Gi
---
apiVersion: v1
kind: Service
metadata:
  name: public-loadbalancer
  namespace: default
  annotations:
    service.beta.kubernetes.io/aws-load-balancer-type: "nlb"
    service.beta.kubernetes.io/aws-load-balancer-scheme: "internet-facing"
spec:
  type: LoadBalancer
  selector:
    app: public-loadbalancer
  ports:
  - port: 80
    targetPort: 8080
    protocol: TCP
```

## Networking Requirements

### Security Groups (AWS)

**Public LoadBalancer Security Group**:
```
Inbound:
- Port 80/443 from 0.0.0.0/0 (Internet)
- Port 8080 from ALB security group

Outbound:
- Port 80/443 to Private Istio Gateway security group
- Port 15021 to Private Istio Gateway (health checks)
```

**Private Istio Gateway Security Group**:
```
Inbound:
- Port 80/443 from Public LoadBalancer security group
- Port 15021 from Public LoadBalancer (health checks)

Outbound:
- All traffic to Kubernetes pod CIDR
```

### VPC Peering (if needed)

If your LoadBalancer and Kubernetes cluster are in different VPCs:

1. Create VPC peering connection
2. Update route tables:
   - Public subnet: Route private cluster CIDR to peering connection
   - Private subnet: Route public subnet CIDR to peering connection
3. Update security groups to allow cross-VPC traffic

## Health Check Endpoints

| Endpoint | Purpose | Response |
|----------|---------|----------|
| `/health/live` | Liveness probe | 200 OK if service is alive |
| `/health/ready` | Readiness probe | 200 OK if all backends healthy |
| `/health` | Legacy health check | 200 OK |
| `/` | Service info | JSON with service details |

## Monitoring

### CloudWatch Metrics (AWS)

Key metrics to monitor:
- `TargetResponseTime`: Time to receive response from Istio gateway
- `HealthyHostCount`: Number of healthy backend instances
- `UnHealthyHostCount`: Number of unhealthy backends
- `RequestCount`: Total requests
- `HTTPCode_Target_5XX_Count`: Backend errors

### Application Logs

The service logs to stdout/stderr in JSON format:

```json
{
  "Timestamp": "2025-12-18 10:30:00",
  "Level": "Information",
  "MessageTemplate": "Proxying {Method} {Path} to backend",
  "Properties": {
    "Method": "GET",
    "Path": "/assetregistry/api/assets"
  }
}
```

## Troubleshooting

### Issue: Cannot connect to Istio gateway

**Symptoms**: 503 Service Unavailable errors

**Solutions**:
1. Verify Istio gateway DNS resolution:
   ```bash
   nslookup istio-ingressgateway.istio-system.svc.cluster.local
   ```

2. Check security groups allow traffic on port 80

3. Verify Istio gateway is healthy:
   ```bash
   curl http://istio-gateway:15021/healthz/ready
   ```

4. Check LoadBalancer logs:
   ```bash
   docker logs loadbalancer 2>&1 | grep ERROR
   ```

### Issue: High latency

**Solutions**:
1. Increase connection pool settings in appsettings.json
2. Add more LoadBalancer replicas
3. Enable keep-alive connections
4. Check network latency between subnets

### Issue: Rate limiting errors

**Symptoms**: 429 Too Many Requests

**Solutions**:
1. Increase rate limit in configuration:
   ```bash
   -e RateLimiter__PermitLimit=500
   ```

2. Implement client-side retry logic
3. Add more backend replicas

## Security Considerations

1. **TLS Termination**: Terminate TLS at ALB/NLB, not in this service
2. **Non-Root User**: Container runs as `appuser` (non-root)
3. **Network Isolation**: Use security groups to restrict access
4. **Secrets Management**: Use AWS Secrets Manager for sensitive config
5. **Rate Limiting**: Enabled by default to prevent abuse
6. **CORS**: Currently allows all origins - restrict in production

## Performance Tuning

### Connection Pooling
```json
"HttpClient": {
  "PooledConnectionLifetime": "00:10:00",
  "PooledConnectionIdleTimeout": "00:05:00",
  "MaxConnectionsPerServer": 100
}
```

### Rate Limiting
```json
"RateLimiter": {
  "PermitLimit": 100,
  "Window": "00:01:00",
  "QueueLimit": 0
}
```

### Resource Limits
- **CPU**: 500m-1000m per replica
- **Memory**: 512Mi-1Gi per replica
- **Replicas**: 2-5 based on traffic

## Testing

### Local Testing (with port forwarding to Istio)

```bash
# Port forward to Istio gateway
kubectl port-forward -n istio-system svc/istio-ingressgateway 8080:80

# Update appsettings.json to use localhost:8080
# Run LoadBalancer
dotnet run --project src/Services/LoadBalancer/LoadBalancer.Api/LoadBalancer.Api.csproj

# Test
curl http://localhost:5000/assetregistry/health
```

### Integration Testing

```bash
# Test health
curl -i http://your-loadbalancer:8080/health/ready

# Test routing
curl -i http://your-loadbalancer:8080/assetregistry/api/assets

# Test rate limiting
for i in {1..150}; do curl http://your-loadbalancer:8080/; done
```

## References

- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [Istio Gateway](https://istio.io/latest/docs/reference/config/networking/gateway/)
- [AWS ECS Networking](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-networking.html)
- [Kubernetes Service Types](https://kubernetes.io/docs/concepts/services-networking/service/#loadbalancer)
