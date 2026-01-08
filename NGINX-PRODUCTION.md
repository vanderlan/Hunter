# Working Nginx Configuration for Hunter Production

## ? Final Working Setup

### Server Nginx Configuration
**Location:** `/etc/nginx/sites-available/hunter`

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
    server_name hunter.geminicode.com.br;

    location / {
        proxy_pass http://localhost:5200;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # API proxy for frontend - FIXED
    location /api/* {
        proxy_pass http://localhost:5010;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Connection 'upgrade';
        proxy_cache_bypass $http_upgrade;
    }

    listen 443 ssl; # managed by Certbot
    ssl_certificate /etc/letsencrypt/live/hunter.geminicode.com.br/fullchain.pem; # managed by Certbot
    ssl_certificate_key /etc/letsencrypt/live/hunter.geminicode.com.br/privkey.pem; # managed by Certbot
    include /etc/letsencrypt/options-ssl-nginx.conf; # managed by Certbot
    ssl_dhparam /etc/letsencrypt/ssl-dhparams.pem; # managed by Certbot
}

server {
    if ($host = hunter.geminicode.com.br) {
        return 301 https://$host$request_uri;
    } # managed by Certbot

    listen 80;
    server_name hunter.geminicode.com.br;
    return 404; # managed by Certbot
}
```

## ?? Key Configuration Points

### 1. API Proxy Pattern
```nginx
location /api/* {
    proxy_pass http://localhost:5010;  # ? No trailing slash, keeps full path
}
```

**Why it works:**
- Request: `https://hunter.geminicode.com.br/api/auth/login`
- Nginx forwards to: `http://localhost:5010/api/auth/login`
- Full path preserved ?

### 2. Container Nginx (Frontend)
**File:** `front-end/nginx.conf`

**Keep simple** - Only serve static files, no API proxy needed:
```nginx
location / {
    try_files $uri $uri/ /index.html;
}
```

Server nginx handles API routing, container nginx just serves Angular app.

## ?? Architecture Flow

```
Internet (HTTPS)
    ?
[Server Nginx] :443 (SSL)
    ?
    ??? /api/* ? localhost:5010 ? [API Container]
    ??? / ? localhost:5200 ? [Frontend Container]
```

## ?? SSL Configuration

- ? Managed by Let's Encrypt (Certbot)
- ? Auto-renewal configured
- ? HTTP to HTTPS redirect working
- ? SSL certificates valid

## ?? URLs

| Service | URL | Backend |
|---------|-----|---------|
| Frontend | https://hunter.geminicode.com.br | Docker port 5200 |
| API | https://hunter.geminicode.com.br/api/* | Docker port 5010 |
| API Direct | http://api.hunter.geminicode.com.br | Docker port 5010 (HTTP only) |

## ??? Maintenance Commands

### Restart Nginx
```sh
sudo systemctl restart nginx
```

### Test Configuration
```sh
sudo nginx -t
```

### View Logs
```sh
# Nginx logs
sudo tail -f /var/log/nginx/error.log
sudo tail -f /var/log/nginx/access.log

# Container logs
docker logs hunter-api -f
docker logs hunter-frontend -f
```

### Reload Configuration (without downtime)
```sh
sudo nginx -s reload
```

### Check SSL Certificate
```sh
sudo certbot certificates
```

### Renew SSL Certificate
```sh
sudo certbot renew
```

## ? Verification

### Test Endpoints
```sh
# Frontend
curl -I https://hunter.geminicode.com.br

# API Health
curl https://hunter.geminicode.com.br/api/health

# Login endpoint
curl -X POST https://hunter.geminicode.com.br/api/auth/login \
  -H "Content-Type: application/json" \
  -H "x-api-version: 1.0" \
  -d '{"email":"adm@hunter.com","password":"123"}'
```

## ?? Troubleshooting

### Issue: 502 Bad Gateway
**Solution:** Check if containers are running
```sh
docker ps --filter "name=hunter-"
docker-compose restart
```

### Issue: SSL Certificate Error
**Solution:** Renew certificate
```sh
sudo certbot renew --force-renewal
sudo systemctl restart nginx
```

### Issue: API 404
**Solution:** Verify proxy_pass doesn't have trailing slash
```nginx
# ? Correct
proxy_pass http://localhost:5010;

# ? Wrong (strips /api)
proxy_pass http://localhost:5010/;
```

## ?? Key Learnings

1. **Trailing slash matters** in `proxy_pass`
   - With `/`: Strips the location path
   - Without `/`: Preserves full path

2. **Wildcard in location** `/api/*`
   - Matches all paths starting with `/api/`
   - More flexible than `/api/` alone

3. **Two-level routing**
   - Server nginx: SSL + routing
   - Container nginx: Static file serving

4. **Server nginx takes precedence**
   - No need for API proxy in container nginx
   - Server nginx handles all API routing

## ?? Final Status

- ? Frontend accessible at https://hunter.geminicode.com.br
- ? API accessible at https://hunter.geminicode.com.br/api/*
- ? SSL certificate valid and auto-renewing
- ? HTTP to HTTPS redirect working
- ? Docker containers running and accessible
- ? Nginx configuration optimized

---

**Configuration Date:** 2024-01-07  
**Status:** Production Ready ?  
**SSL Expiry:** Check with `sudo certbot certificates`
