# Quick Start: Deploy LoadBalancer in AWS Public Subnet

This guide will help you quickly deploy the LoadBalancer service in an AWS public subnet routing to your private Kubernetes/Istio cluster.

## Prerequisites

- ✅ AWS Account with VPC configured
- ✅ Public subnets with Internet Gateway route
- ✅ Private subnets with your Kubernetes/Istio cluster
- ✅ Internal NLB for Istio ingress gateway (in private subnet)
- ✅ Docker installed locally
- ✅ AWS CLI configured

## Step 1: Get Your Istio Gateway Address

Find your internal Istio gateway load balancer DNS:

```bash
# Get Istio gateway service
kubectl get svc -n istio-system istio-ingressgateway

# If using AWS, find the internal NLB DNS
aws elbv2 describe-load-balancers \
  --query 'LoadBalancers[?Scheme==`internal`].DNSName' \
  --output text
```

**Example output:**
```
internal-istio-k8s-istiosys-abcd1234-1234567890.elb.us-east-1.amazonaws.com
```

Save this address - you'll need it in Step 4.

## Step 2: Build and Push Docker Image

### Option A: AWS ECR

```bash
# Authenticate to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com

# Create repository (first time only)
aws ecr create-repository --repository-name loadbalancer --region us-east-1

# Build image
cd d:\Github_repos\Asset-Registry-and-Tracking-System-
docker build -t loadbalancer:latest -f src/Services/LoadBalancer/Dockerfile .

# Tag image
docker tag loadbalancer:latest YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/loadbalancer:latest

# Push image
docker push YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/loadbalancer:latest
```

### Option B: GitHub Container Registry (GHCR)

```bash
# Authenticate to GHCR
echo $GITHUB_TOKEN | docker login ghcr.io -u YOUR_GITHUB_USERNAME --password-stdin

# Build image
cd d:\Github_repos\Asset-Registry-and-Tracking-System-
docker build -t loadbalancer:latest -f src/Services/LoadBalancer/Dockerfile .

# Tag image
docker tag loadbalancer:latest ghcr.io/YOUR_ORG/loadbalancer:latest

# Push image
docker push ghcr.io/YOUR_ORG/loadbalancer:latest
```

## Step 3: Configure Security Groups

### Create Security Groups

```bash
# Get your VPC ID
VPC_ID=$(aws ec2 describe-vpcs --filters "Name=tag:Name,Values=your-vpc-name" --query 'Vpcs[0].VpcId' --output text)

# Create Public LoadBalancer Security Group
PUB_LB_SG=$(aws ec2 create-security-group \
  --group-name public-loadbalancer-sg \
  --description "Security group for public LoadBalancer" \
  --vpc-id $VPC_ID \
  --query 'GroupId' \
  --output text)

echo "Public LB Security Group: $PUB_LB_SG"
```

### Configure Security Group Rules

```bash
# Allow inbound HTTP from internet (will be from ALB in production)
aws ec2 authorize-security-group-ingress \
  --group-id $PUB_LB_SG \
  --protocol tcp \
  --port 8080 \
  --cidr 0.0.0.0/0

# Allow outbound to Istio gateway (get Istio SG ID first)
ISTIO_SG_ID="sg-xxxxx"  # Replace with your Istio gateway security group

# Allow LoadBalancer to reach Istio gateway
aws ec2 authorize-security-group-ingress \
  --group-id $ISTIO_SG_ID \
  --protocol tcp \
  --port 80 \
  --source-group $PUB_LB_SG

aws ec2 authorize-security-group-ingress \
  --group-id $ISTIO_SG_ID \
  --protocol tcp \
  --port 443 \
  --source-group $PUB_LB_SG

# Allow health checks
aws ec2 authorize-security-group-ingress \
  --group-id $ISTIO_SG_ID \
  --protocol tcp \
  --port 15021 \
  --source-group $PUB_LB_SG
```

## Step 4: Deploy to AWS ECS (Recommended)

### Create ECS Cluster

```bash
# Create cluster in public subnets
aws ecs create-cluster --cluster-name public-loadbalancer-cluster
```

### Create Task Execution Role (First Time Only)

```bash
# Create role
aws iam create-role \
  --role-name ecsTaskExecutionRole \
  --assume-role-policy-document '{
    "Version": "2012-10-17",
    "Statement": [{
      "Effect": "Allow",
      "Principal": {"Service": "ecs-tasks.amazonaws.com"},
      "Action": "sts:AssumeRole"
    }]
  }'

# Attach policy
aws iam attach-role-policy \
  --role-name ecsTaskExecutionRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
```

### Update and Register Task Definition

```bash
# Edit aws-ecs-task-definition.json
# Replace:
# - YOUR_ACCOUNT_ID with your AWS account ID
# - YOUR_ECR_REPO with your ECR repository
# - internal-istio-nlb-xxxxx with your actual Istio NLB DNS

# Register task definition
cd src/Services/LoadBalancer
aws ecs register-task-definition --cli-input-json file://aws-ecs-task-definition.json
```

### Create Application Load Balancer

```bash
# Get public subnet IDs
PUBLIC_SUBNET_1="subnet-xxxxx"
PUBLIC_SUBNET_2="subnet-yyyyy"

# Create ALB
ALB_ARN=$(aws elbv2 create-load-balancer \
  --name public-loadbalancer-alb \
  --subnets $PUBLIC_SUBNET_1 $PUBLIC_SUBNET_2 \
  --security-groups $PUB_LB_SG \
  --scheme internet-facing \
  --type application \
  --query 'LoadBalancers[0].LoadBalancerArn' \
  --output text)

echo "ALB ARN: $ALB_ARN"

# Create target group
TG_ARN=$(aws elbv2 create-target-group \
  --name public-loadbalancer-tg \
  --protocol HTTP \
  --port 8080 \
  --vpc-id $VPC_ID \
  --target-type ip \
  --health-check-enabled \
  --health-check-path /health/ready \
  --health-check-interval-seconds 30 \
  --healthy-threshold-count 2 \
  --unhealthy-threshold-count 3 \
  --query 'TargetGroups[0].TargetGroupArn' \
  --output text)

echo "Target Group ARN: $TG_ARN"

# Create listener
aws elbv2 create-listener \
  --load-balancer-arn $ALB_ARN \
  --protocol HTTP \
  --port 80 \
  --default-actions Type=forward,TargetGroupArn=$TG_ARN
```

### Create ECS Service

```bash
# Get public subnet IDs (same as above)
PUBLIC_SUBNET_1="subnet-xxxxx"
PUBLIC_SUBNET_2="subnet-yyyyy"

# Create service
aws ecs create-service \
  --cluster public-loadbalancer-cluster \
  --service-name loadbalancer-service \
  --task-definition public-loadbalancer:1 \
  --desired-count 2 \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[$PUBLIC_SUBNET_1,$PUBLIC_SUBNET_2],securityGroups=[$PUB_LB_SG],assignPublicIp=ENABLED}" \
  --load-balancers "targetGroupArn=$TG_ARN,containerName=loadbalancer,containerPort=8080"
```

## Step 5: Verify Deployment

### Check ECS Service Status

```bash
# Check service
aws ecs describe-services \
  --cluster public-loadbalancer-cluster \
  --services loadbalancer-service

# Check tasks
aws ecs list-tasks \
  --cluster public-loadbalancer-cluster \
  --service-name loadbalancer-service
```

### Get ALB DNS Name

```bash
ALB_DNS=$(aws elbv2 describe-load-balancers \
  --load-balancer-arns $ALB_ARN \
  --query 'LoadBalancers[0].DNSName' \
  --output text)

echo "ALB DNS: $ALB_DNS"
```

### Test Endpoints

```bash
# Health check
curl -i http://$ALB_DNS/health/ready

# Service info
curl http://$ALB_DNS/

# Test routing through to backend
curl -i http://$ALB_DNS/assetregistry/health
curl -i http://$ALB_DNS/identity/health
```

## Step 6: Configure DNS (Optional)

```bash
# Create Route 53 record pointing to ALB
HOSTED_ZONE_ID="Z1234567890ABC"
DOMAIN_NAME="api.yourdomain.com"

aws route53 change-resource-record-sets \
  --hosted-zone-id $HOSTED_ZONE_ID \
  --change-batch '{
    "Changes": [{
      "Action": "UPSERT",
      "ResourceRecordSet": {
        "Name": "'$DOMAIN_NAME'",
        "Type": "A",
        "AliasTarget": {
          "HostedZoneId": "Z35SXDOTRQ7X7K",
          "DNSName": "'$ALB_DNS'",
          "EvaluateTargetHealth": true
        }
      }
    }]
  }'
```

## Alternative: Deploy to Kubernetes

If you prefer Kubernetes deployment:

```bash
# Update k8s-public-deployment.yaml with your values
# - Image: your-registry/loadbalancer:latest
# - Istio gateway address

# Apply deployment
kubectl apply -f src/Services/LoadBalancer/k8s-public-deployment.yaml

# Get LoadBalancer external IP
kubectl get svc public-loadbalancer -w

# Test
EXTERNAL_IP=$(kubectl get svc public-loadbalancer -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')
curl http://$EXTERNAL_IP/health/ready
```

## Troubleshooting

### Tasks Not Starting

```bash
# Check task logs
TASK_ARN=$(aws ecs list-tasks --cluster public-loadbalancer-cluster --service loadbalancer-service --query 'taskArns[0]' --output text)

aws ecs describe-tasks --cluster public-loadbalancer-cluster --tasks $TASK_ARN
```

### Cannot Connect to Istio Gateway

```bash
# Test connectivity from ECS task to Istio
# Get task private IP
TASK_IP=$(aws ecs describe-tasks \
  --cluster public-loadbalancer-cluster \
  --tasks $TASK_ARN \
  --query 'tasks[0].attachments[0].details[?name==`privateIPv4Address`].value' \
  --output text)

# Try to curl Istio from your local machine (via VPN or bastion)
curl -v http://internal-istio-nlb.elb.amazonaws.com/healthz/ready
```

### High Error Rate

```bash
# Check CloudWatch logs
aws logs tail /ecs/public-loadbalancer --follow

# Check target health
aws elbv2 describe-target-health --target-group-arn $TG_ARN
```

## Monitoring Setup

### Create CloudWatch Dashboard

```bash
aws cloudwatch put-dashboard \
  --dashboard-name LoadBalancer-Monitoring \
  --dashboard-body file://cloudwatch-dashboard.json
```

### Set Up Alarms

```bash
# High error rate alarm
aws cloudwatch put-metric-alarm \
  --alarm-name loadbalancer-high-errors \
  --alarm-description "Alert on high 5xx error rate" \
  --metric-name HTTPCode_Target_5XX_Count \
  --namespace AWS/ApplicationELB \
  --statistic Sum \
  --period 300 \
  --threshold 10 \
  --comparison-operator GreaterThanThreshold \
  --dimensions Name=LoadBalancer,Value=$(echo $ALB_ARN | cut -d: -f6) \
  --evaluation-periods 1

# Unhealthy target alarm
aws cloudwatch put-metric-alarm \
  --alarm-name loadbalancer-unhealthy-targets \
  --alarm-description "Alert on unhealthy targets" \
  --metric-name UnHealthyHostCount \
  --namespace AWS/ApplicationELB \
  --statistic Average \
  --period 60 \
  --threshold 1 \
  --comparison-operator GreaterThanThreshold \
  --dimensions Name=TargetGroup,Value=$(echo $TG_ARN | cut -d: -f6) \
  --evaluation-periods 2
```

## Clean Up (If Testing)

```bash
# Delete ECS service
aws ecs delete-service --cluster public-loadbalancer-cluster --service loadbalancer-service --force

# Delete ALB
aws elbv2 delete-load-balancer --load-balancer-arn $ALB_ARN

# Delete target group (wait for ALB deletion first)
aws elbv2 delete-target-group --target-group-arn $TG_ARN

# Delete cluster
aws ecs delete-cluster --cluster public-loadbalancer-cluster

# Delete security group
aws ec2 delete-security-group --group-id $PUB_LB_SG
```

## Next Steps

1. ✅ Configure HTTPS/TLS at ALB level
2. ✅ Set up WAF rules for security
3. ✅ Configure auto-scaling based on metrics
4. ✅ Set up proper DNS with your domain
5. ✅ Configure backup and disaster recovery
6. ✅ Implement monitoring and alerting
7. ✅ Set up CI/CD pipeline for automatic deployments

## Support

For detailed configuration options, see:
- [CONFIGURATION_SUMMARY.md](CONFIGURATION_SUMMARY.md) - Complete configuration reference
- [DEPLOYMENT_GUIDE.md](LoadBalancer.Api/DEPLOYMENT_GUIDE.md) - Comprehensive deployment guide
- [README.md](LoadBalancer.Api/README.md) - Service documentation
