# Hunter Production Deployment Guide

## ?? Prerequisites

- Production server with Docker and Docker Compose installed
- Domain `hunter.geminicode.com.br` pointing to your server
- SSH access to the server
- All GitHub secrets configured

## ?? Initial Server Setup

### 1. Connect to Production Server

```bash
ssh user@hunter.geminicode.com.br
```

### 2. Install Docker (if not installed)

```bash
# Update packages
sudo apt update
sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add user to docker group
sudo usermod -aG docker $USER

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Verify installation
docker --version
docker-compose --version
```

### 3. Create Project Directory

```bash
mkdir -p ~/projects/hunter
cd ~/projects/hunter
```

### 4. Create Environment File

```bash
# Copy the .env.production.example content
nano .env

# Set proper permissions
chmod 600 .env
```

**Edit the `.env` file with your actual values:**

```env
POSTGRES_PASSWORD=<your-strong-password>
JWT_SECRET_KEY=<your-256-bit-key>
OPENAI_API_KEY=<your-openai-key>
EMAIL_PASSWORD=<your-gmail-app-password>
```

### 5. Create docker-compose.yml

```bash
# Copy docker-compose.prod.yml content
nano docker-compose.yml
```

### 6. Create deploy.sh Script

```bash
# Copy deploy.sh content
nano deploy.sh

# Make it executable
chmod +x deploy.sh
```

### 7. Setup Nginx Reverse Proxy (Optional but Recommended)

```bash
# Install Nginx
sudo apt install nginx -y

# Create Nginx configuration
sudo nano /etc/nginx/sites-available/hunter
```

Add this configuration:

```nginx
# API Backend
server {
    listen 80;
    server_name api.hunter.geminicode.com.br;

    location / {
        proxy_pass http://localhost:5010;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# Frontend
server {
    listen 80;
    server_name hunter.geminicode.com.br;

    location / {
        proxy_pass http://localhost:5200;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # API proxy for frontend
    location /api/ {
        proxy_pass http://localhost:5010/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable the site:

```bash
# Create symbolic link
sudo ln -s /etc/nginx/sites-available/hunter /etc/nginx/sites-enabled/

# Test configuration
sudo nginx -t

# Restart Nginx
sudo systemctl restart nginx
```

### 8. Setup SSL with Let's Encrypt (Recommended)

```bash
# Install Certbot
sudo apt install certbot python3-certbot-nginx -y

# Obtain certificates
sudo certbot --nginx -d hunter.geminicode.com.br -d api.hunter.geminicode.com.br

# Auto-renewal is configured by default
# Test renewal
sudo certbot renew --dry-run
```

## ?? First Deployment

### Option 1: Via GitHub Actions (Automatic)

Just push to master or create a tag:

```bash
# Automatic deployment on push
git push origin master

# Or create a version tag
git tag v1.0.0
git push origin v1.0.0
```

### Option 2: Manual Deployment

```bash
cd ~/projects/hunter

# Pull images manually
docker-compose pull

# Start services
docker-compose up -d

# Check logs
docker-compose logs -f

# Run migrations
docker exec hunter-api dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api
```

## ?? Verify Deployment

### Check Services

```bash
# Check running containers
docker ps

# Check logs
docker-compose logs -f api
docker-compose logs -f frontend

# Check health
curl http://localhost:5010/health
curl http://localhost:5200
```

### Access Application

- **Frontend**: https://hunter.geminicode.com.br
- **API**: https://hunter.geminicode.com.br/api/health
- **Swagger**: https://hunter.geminicode.com.br/api/swagger
- **Kibana**: http://hunter.geminicode.com.br:5601

## ??? Common Operations

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f frontend
docker-compose logs -f postgresql
```

### Restart Services

```bash
# All services
docker-compose restart

# Specific service
docker-compose restart api
docker-compose restart frontend
```

### Update Application

```bash
cd ~/projects/hunter
./deploy.sh
```

### Database Backup

```bash
# Create backup
docker exec postgresql pg_dump -U postgres HunterProd > backup_$(date +%Y%m%d_%H%M%S).sql

# Restore backup
docker exec -i postgresql psql -U postgres HunterProd < backup_20240107_120000.sql
```

### Clean Up

```bash
# Remove unused images
docker system prune -a -f

# Remove specific images
docker rmi vanderlangs/hunter-api:latest
docker rmi vanderlangs/hunter-frontend:latest
```

## ?? Security Checklist

- [ ] `.env` file has proper permissions (chmod 600)
- [ ] Database password is strong
- [ ] JWT secret is 256-bit random key
- [ ] SSL certificates are installed
- [ ] Firewall configured (allow 80, 443, SSH only)
- [ ] SSH key authentication enabled
- [ ] Regular backups scheduled
- [ ] Monitoring setup (optional)

## ?? Monitoring

### Check Application Health

```bash
# API Health
curl https://hunter.geminicode.com.br/api/health

# Database
docker exec postgresql pg_isready -U postgres

# Elasticsearch
curl http://localhost:9200/_cluster/health
```

### Resource Usage

```bash
# Container stats
docker stats

# Disk usage
df -h
docker system df
```

## ?? Troubleshooting

### Containers Won't Start

```bash
# Check logs
docker-compose logs

# Verify .env file
cat .env | grep -v PASSWORD | grep -v KEY

# Check Docker
docker ps -a
docker system df
```

### Database Connection Issues

```bash
# Check PostgreSQL
docker exec postgresql pg_isready -U postgres

# Check connection from API
docker exec hunter-api dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api
```

### Port Already in Use

```bash
# Find process using port
sudo lsof -i :5010
sudo lsof -i :5200

# Kill process
sudo kill -9 <PID>
```

## ?? Maintenance

### Regular Tasks

1. **Weekly**: Check logs for errors
2. **Weekly**: Verify backups
3. **Monthly**: Update system packages
4. **Monthly**: Review disk usage
5. **Quarterly**: Update Docker images
6. **Quarterly**: Rotate secrets

### Update System

```bash
# Update packages
sudo apt update
sudo apt upgrade -y

# Update Docker images
cd ~/projects/hunter
docker-compose pull
./deploy.sh
```

## ?? Production Environment Variables

Required secrets configured in GitHub:
- ? DOCKERHUB_USERNAME
- ? DOCKERHUB_TOKEN
- ? HUNTER_PROD_HOST
- ? HUNTER_PROD_USER
- ? HUNTER_PROD_PASSWORD
- ? HUNTER_PROD_SSH_PORT
- ? SONAR_TOKEN (disabled)
- ? ELASTIC_PASSWORD (not required)

## ?? Support

For issues:
1. Check logs: `docker-compose logs -f`
2. Verify health endpoints
3. Check GitHub Actions for deployment status
4. Review this documentation

---

**Production URL**: https://hunter.geminicode.com.br  
**Deployment**: Automated via GitHub Actions on push to master
