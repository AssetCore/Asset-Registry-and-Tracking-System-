# VPC Cross-Subnet Configuration Guide

## Your Setup
- **LoadBalancer**: Public subnet (internet-facing)
- **Istio Gateway**: Private subnet (ClusterIP: 10.108.46.114)
- **Same VPC**: Cross-subnet communication

## Current Configuration
The LoadBalancer is configured to access Istio at: `http://10.108.46.114`

## Important: Network Requirements

### Security Group Configuration

**LoadBalancer Security Group (Public Subnet):**
```
Inbound:
- Port 8080/8443 from 0.0.0.0/0 (Internet traffic)

Outbound:
- Port 80, 443, 15021 to 10.108.46.114 (Istio Gateway)
- Or: All traffic to private subnet CIDR (e.g., 10.0.2.0/24)
```

**Kubernetes Node Security Group (Private Subnet):**
```
Inbound:
- Port 80, 443 from LoadBalancer security group
- Port 15021 from LoadBalancer security group (health checks)

Outbound:
- All traffic (default)
```

### Route Table Configuration

**Public Subnet Route Table:**
```
Destination         Target
0.0.0.0/0          igw-xxxxx (Internet Gateway)
10.0.0.0/16        local (VPC CIDR - includes private subnet)
```

**Private Subnet Route Table:**
```
Destination         Target
0.0.0.0/0          nat-xxxxx (NAT Gateway for outbound)
10.0.0.0/16        local (VPC CIDR)
```

## Testing Network Connectivity

### 1. Test from LoadBalancer to Istio

```bash
# If LoadBalancer is on EC2/ECS
aws ssm start-session --target i-xxxxx  # Your LoadBalancer instance

# Test connectivity
curl -v http://10.108.46.114:15021/healthz/ready
curl -v http://10.108.46.114:80/

# If successful, you should see 200 OK
```

### 2. Check Security Groups

```bash
# Get LoadBalancer security group
LB_SG="sg-xxxxx"  # Your LoadBalancer security group ID

# Get Kubernetes nodes security group
aws ec2 describe-instances \
  --filters "Name=tag:kubernetes.io/cluster/YOUR_CLUSTER,Values=owned" \
  --query 'Reservations[].Instances[].SecurityGroups[].GroupId' \
  --output text

K8S_SG="sg-yyyyy"  # Your K8s nodes security group

# Verify outbound rule on LoadBalancer SG
aws ec2 describe-security-groups --group-ids $LB_SG \
  --query 'SecurityGroups[].IpPermissionsEgress'

# Verify inbound rule on K8s SG allows from LoadBalancer
aws ec2 describe-security-groups --group-ids $K8S_SG \
  --query 'SecurityGroups[].IpPermissions'
```

### 3. Add Security Group Rules if Needed

```bash
# Allow LoadBalancer to reach Istio on ports 80, 443, 15021
aws ec2 authorize-security-group-ingress \
  --group-id $K8S_SG \
  --protocol tcp \
  --port 80 \
  --source-group $LB_SG

aws ec2 authorize-security-group-ingress \
  --group-id $K8S_SG \
  --protocol tcp \
  --port 443 \
  --source-group $LB_SG

aws ec2 authorize-security-group-ingress \
  --group-id $K8S_SG \
  --protocol tcp \
  --port 15021 \
  --source-group $LB_SG
```

## Alternative: Use Internal NLB (Recommended)

Instead of using ClusterIP directly, create an internal NLB in the private subnet:

### Create Internal NLB for Istio

```bash
# Update Istio gateway service to use internal NLB
kubectl patch svc istio-ingressgateway -n istio-system -p '{"metadata":{"annotations":{"service.beta.kubernetes.io/aws-load-balancer-type":"nlb","service.beta.kubernetes.io/aws-load-balancer-internal":"true"}}}'

# Wait for NLB to provision
kubectl get svc istio-ingressgateway -n istio-system -w

# Get the internal NLB DNS
ISTIO_NLB=$(kubectl get svc istio-ingressgateway -n istio-system -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')
echo "Istio Internal NLB: $ISTIO_NLB"
```

### Update LoadBalancer Configuration

Once the internal NLB is created, update your environment variables:

```bash
# Use the NLB DNS instead of ClusterIP
ReverseProxy__Clusters__asset-registry__Destinations__istio-gateway__Address=http://$ISTIO_NLB
```

**Benefits of Internal NLB:**
- ✅ More stable (DNS instead of IP)
- ✅ Built-in health checks
- ✅ Cross-AZ load balancing
- ✅ Automatic failover
- ✅ Better for production

## Current Setup (ClusterIP Direct Access)

**Advantages:**
- ✅ No additional cost
- ✅ Direct connection (lower latency)
- ✅ Simpler setup

**Disadvantages:**
- ⚠️ ClusterIP can change if service is recreated
- ⚠️ Requires proper VPC routing and security groups
- ⚠️ No built-in load balancing across nodes
- ⚠️ Must ensure LoadBalancer can reach K8s pod network

## Verification Checklist

- [ ] VPC routing allows communication between subnets
- [ ] Security groups allow traffic on ports 80, 443, 15021
- [ ] Network ACLs don't block traffic (if configured)
- [ ] LoadBalancer can resolve/reach 10.108.46.114
- [ ] Istio gateway is listening on all interfaces (0.0.0.0)
- [ ] Test with curl from LoadBalancer instance

## Deploy and Test

```bash
# Deploy LoadBalancer
docker run -d \
  -p 8080:8080 \
  -p 8443:8443 \
  --name loadbalancer \
  -e ASPNETCORE_ENVIRONMENT=Production \
  loadbalancer:latest

# Test health endpoint
curl http://localhost:8080/health/ready

# Test routing to Istio
curl http://localhost:8080/assetregistry/health

# Check logs for connectivity issues
docker logs loadbalancer 2>&1 | grep -i "health\|error\|istio"
```

## Troubleshooting

### Connection Refused
**Cause:** Security group blocking traffic
**Fix:** Add security group rules as shown above

### Connection Timeout
**Cause:** No route between subnets or Network ACL blocking
**Fix:** Check route tables and NACLs

### DNS Resolution Failed
**Cause:** N/A (using IP address)
**Fix:** Consider using internal NLB instead

### Health Check Failing
```bash
# Check Istio gateway is healthy
kubectl get pods -n istio-system
kubectl logs -n istio-system deployment/istio-ingressgateway

# Test health endpoint directly
kubectl port-forward -n istio-system svc/istio-ingressgateway 15021:15021
curl http://localhost:15021/healthz/ready
```

## Recommended Production Setup

For production, use internal NLB instead of direct ClusterIP access:

1. Create internal NLB for Istio (shown above)
2. Update LoadBalancer config with NLB DNS
3. Configure proper security groups
4. Set up monitoring and alarms

This provides better reliability and follows AWS best practices.
