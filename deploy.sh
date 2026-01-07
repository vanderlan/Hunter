#!/bin/bash

# Hunter Production Deployment Script
# This script should be placed at: ~/projects/hunter/deploy.sh on production server

set -e  # Exit on error

echo "?? Starting Hunter deployment..."

# Pull latest images from DockerHub
echo "?? Pulling latest Docker images..."
docker-compose pull

# Stop and remove old containers
echo "?? Stopping old containers..."
docker-compose down

# Start new containers in detached mode
echo "? Starting new containers..."
docker-compose up -d

# Wait for services to be healthy
echo "? Waiting for services to start..."
sleep 10

# Clean up unused Docker resources
echo "?? Cleaning up Docker resources..."
docker system prune -f

# Show running containers
echo "? Deployment complete! Running containers:"
docker ps --filter "name=hunter-"

echo ""
echo "?? Service Status:"
echo "  API: https://hunter.geminicode.com.br/api/health"
echo "  Frontend: https://hunter.geminicode.com.br"
echo ""
echo "?? Deployment successful!"
