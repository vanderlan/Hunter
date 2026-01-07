# Frontend Nginx Configuration Fix

## Problem
The frontend was getting 404 errors when trying to access the API at `https://hunter.geminicode.com.br/api/auth/login`.

## Root Cause
The frontend nginx configuration didn't have a proxy setup to forward `/api/*` requests to the backend container.

## Solution Applied

Updated `front-end/nginx.conf` to include:

```nginx
location /api/ {
    proxy_pass http://hunter-api:80/;
    # ... proxy headers and CORS configuration
}
```

This configuration:
1. Intercepts requests to `/api/*`
2. Forwards them to the backend container (`hunter-api:80`)
3. Handles CORS headers
4. Manages preflight OPTIONS requests

## How It Works

```
Browser Request:
https://hunter.geminicode.com.br/api/auth/login
               ?
Frontend Container (nginx on port 80)
               ?
Nginx intercepts /api/ requests
               ?
Proxy to: http://hunter-api:80/auth/login
               ?
Backend API Container processes request
               ?
Response back through nginx to browser
```

## Deployment Steps

### 1. Rebuild Frontend Image

```sh
cd front-end
docker build -t vanderlangs/hunter-frontend:latest .
```

### 2. Push to DockerHub

```sh
docker push vanderlangs/hunter-frontend:latest
```

### 3. Deploy to Production

**Option A: Via GitHub Actions (Recommended)**
```sh
git add front-end/nginx.conf
git commit -m "fix: add API proxy to frontend nginx configuration"
git push origin master
```

**Option B: Manual Deployment**
```sh
# SSH to server
ssh user@hunter.geminicode.com.br

# Navigate to project
cd ~/projects/hunter

# Pull new image
docker-compose pull frontend

# Restart frontend
docker-compose up -d frontend

# Verify
curl -I https://hunter.geminicode.com.br/api/health
```

## Verification

### 1. Check Frontend Container Logs
```sh
docker logs hunter-frontend -f
```

### 2. Test API Endpoint
```sh
# From server
curl -X POST http://localhost:5200/api/auth/login \
  -H "Content-Type: application/json" \
  -H "x-api-version: 1.0" \
  -d '{"email":"test@test.com","password":"test"}'

# From browser
# Open https://hunter.geminicode.com.br
# Try to login
```

### 3. Check Network Communication
```sh
# Enter frontend container
docker exec -it hunter-frontend sh

# Test connection to backend
wget -O- http://hunter-api:80/health

# Should return API health status
```

## Alternative Solution (Not Recommended)

If you prefer, you could change the frontend environment to point directly to the API port:

```typescript
// front-end/src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://hunter.geminicode.com.br:5010/api', // Direct to API port
  apiVersion: '1.0'
};
```

But this requires:
1. Exposing port 5010 publicly
2. Configuring CORS on the backend
3. SSL certificate on both ports
4. Less secure architecture

**The nginx proxy solution (implemented above) is the recommended approach.**

## Configuration Details

### Proxy Settings Explained

```nginx
proxy_pass http://hunter-api:80/;
```
- `hunter-api` - Docker container name (from docker-compose)
- Port `80` - Internal container port
- Trailing `/` - Removes `/api/` from forwarded URL

### Example Request Flow

**Client Request:**
```
GET https://hunter.geminicode.com.br/api/auth/login
```

**Nginx Processes:**
```
1. Matches location /api/
2. Strips /api/ prefix
3. Forwards to: http://hunter-api:80/auth/login
4. API responds
5. Nginx returns response to client
```

### CORS Configuration

The nginx configuration includes CORS headers to allow:
- All origins (for development)
- Standard HTTP methods
- Custom headers (x-api-version, Authorization)
- Preflight OPTIONS requests

**For production**, you should restrict CORS:
```nginx
add_header 'Access-Control-Allow-Origin' 'https://hunter.geminicode.com.br' always;
```

## Troubleshooting

### Still Getting 404?

1. **Check container names:**
```sh
docker ps --filter "name=hunter-"
# Should show: hunter-api and hunter-frontend
```

2. **Check nginx syntax:**
```sh
docker exec hunter-frontend nginx -t
```

3. **Reload nginx config:**
```sh
docker exec hunter-frontend nginx -s reload
```

4. **Check backend is responding:**
```sh
docker exec hunter-api curl http://localhost:80/health
```

### CORS Errors?

If you see CORS errors in browser console:
1. Check backend CORS configuration
2. Verify nginx CORS headers are applied
3. Check browser network tab for preflight requests

## Status

- ? Nginx configuration updated
- ? API proxy configured
- ? CORS headers added
- ? Ready to deploy

## Next Steps

1. Commit and push changes
2. GitHub Actions will automatically:
   - Build new frontend image
   - Push to DockerHub
   - Deploy to production
   - Run health checks
3. Test login at https://hunter.geminicode.com.br

---

**Updated**: 2024-01-07  
**Status**: Ready to deploy ??
