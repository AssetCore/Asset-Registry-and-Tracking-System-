# TLS Certificate Setup for LoadBalancer

This LoadBalancer is configured to handle HTTPS traffic directly. You need to provide a TLS certificate.

## Certificate Options

### Option 1: Kubernetes with cert-manager (Recommended)

**Install cert-manager:**
```bash
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml
```

**Create ClusterIssuer for Let's Encrypt:**
```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: your-email@example.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
```

**Create Certificate:**
```yaml
apiVersion: cert-manager.io/v1
kind: Certificate
metadata:
  name: loadbalancer-tls
  namespace: default
spec:
  secretName: loadbalancer-tls
  issuerRef:
    name: letsencrypt-prod
    kind: ClusterIssuer
  dnsNames:
  - api.yourdomain.com
  - www.api.yourdomain.com
```

**Apply:**
```bash
kubectl apply -f cluster-issuer.yaml
kubectl apply -f certificate.yaml
```

The certificate will be automatically created in the `loadbalancer-tls` secret and mounted to `/app/certs`.

### Option 2: Manual Certificate (PFX format)

**Generate self-signed certificate (for testing):**
```bash
# Generate certificate
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes \
  -subj "/CN=api.yourdomain.com"

# Convert to PFX
openssl pkcs12 -export -out aspnetapp.pfx -inkey key.pem -in cert.pem -password pass:YourPassword
```

**Create Kubernetes secret:**
```bash
kubectl create secret generic loadbalancer-tls \
  --from-file=aspnetapp.pfx=./aspnetapp.pfx \
  --namespace default
```

**Update deployment environment variable:**
```yaml
- name: ASPNETCORE_Kestrel__Certificates__Default__Password
  value: "YourPassword"
```

### Option 3: AWS Certificate Manager (ECS/EC2)

**For ECS with ALB:**
```bash
# Request certificate
aws acm request-certificate \
  --domain-name api.yourdomain.com \
  --validation-method DNS \
  --subject-alternative-names *.api.yourdomain.com

# Add HTTPS listener to ALB
aws elbv2 create-listener \
  --load-balancer-arn $ALB_ARN \
  --protocol HTTPS \
  --port 443 \
  --certificates CertificateArn=arn:aws:acm:region:account:certificate/xxx \
  --default-actions Type=forward,TargetGroupArn=$TG_ARN
```

### Option 4: Docker with Certificate File

**Mount certificate as volume:**
```bash
docker run -d \
  -p 8080:8080 \
  -p 8443:8443 \
  -v /path/to/certs:/app/certs:ro \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certs/aspnetapp.pfx \
  -e ASPNETCORE_Kestrel__Certificates__Default__Password=YourPassword \
  loadbalancer:latest
```

## Development Certificate

For local development, use the ASP.NET Core dev certificate:

```bash
# Generate dev certificate
dotnet dev-certs https --trust

# Run with HTTPS
dotnet run --launch-profile https
```

## Verify HTTPS Configuration

```bash
# Check HTTP redirect
curl -i http://your-domain:8080/health

# Should redirect to HTTPS with 307 or 308

# Check HTTPS
curl -k https://your-domain:8443/health
```

## Security Notes

1. **Never commit certificates to git**
2. **Use strong passwords for PFX files**
3. **Rotate certificates before expiration**
4. **Use Let's Encrypt for production**
5. **Enable HSTS headers in production**

## Kubernetes Secret Format

The deployment expects a secret named `loadbalancer-tls` with this structure:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: loadbalancer-tls
  namespace: default
type: Opaque
data:
  aspnetapp.pfx: <base64-encoded-pfx-file>
```

Or create from file:
```bash
kubectl create secret generic loadbalancer-tls \
  --from-file=aspnetapp.pfx=/path/to/cert.pfx \
  --dry-run=client -o yaml | kubectl apply -f -
```

## Troubleshooting

**Certificate not found:**
- Verify secret exists: `kubectl get secret loadbalancer-tls`
- Check volume mount: `kubectl describe pod <pod-name>`
- Verify file exists in container: `kubectl exec <pod> -- ls -la /app/certs`

**HTTPS not working:**
- Check certificate password matches
- Verify certificate isn't expired
- Check firewall rules allow port 8443
- Review logs: `kubectl logs <pod>`
