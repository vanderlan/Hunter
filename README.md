# **Hunter CV Analyzer**

[![Build and Test](https://github.com/vanderlan/Hunter-Api/actions/workflows/build.yml/badge.svg)](https://github.com/vanderlan/Hunter-Api/actions/workflows/build.yml)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=vanderlan_Hunter-Api&metric=coverage)](https://sonarcloud.io/summary/overall?id=vanderlan_Hunter-Api) 
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=vanderlan_Hunter-Api&metric=alert_status)](https://sonarcloud.io/summary/overall?id=vanderlan_Hunter-Api)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=vanderlan_Hunter-Api&metric=sqale_index)](https://sonarcloud.io/summary/overall?id=vanderlan_Hunter-Api)
[![GitHub release](https://img.shields.io/github/release/vanderlan/Hunter-Api.svg)](https://github.com/vanderlan/Hunter-Api/releases) 

## About this Project

**Hunter CV Analyzer** is a comprehensive AI-powered system for analyzing CVs against job positions. It uses OpenAI GPT-4o to intelligently match candidates with available jobs, providing match percentages, skills analysis, and detailed recommendations.

### Key Features

+ **AI-Powered CV Analysis**
	+ OpenAI GPT-4o integration for intelligent matching
	+ PDF text extraction and analysis
	+ Match percentage calculation (>20% threshold)
	+ Skills gap analysis
	+ Matching and missing skills identification

+ **Job Management**
	+ Automated web scraping from external job boards
	+ Background service for hourly synchronization (8 AM - 6 PM)
	+ Email notifications on job changes
	+ Job position tracking and history

+ **Modern Tech Stack**
	+ .NET 10 Web API backend
	+ Angular 17 standalone components frontend
	+ PostgreSQL database
	+ Elasticsearch + Kibana for logging
	+ Docker Compose for containerization

## ?? Quick Start with Docker

**Start the entire system with one command:**

```bash
docker-compose up -d
```

**Access the application:**
- Frontend: http://localhost:4200
- API: http://localhost:5010
- Swagger: http://localhost:5010/swagger
- Kibana (Logs): http://localhost:5601

**Login credentials:**
- Email: `adm@hunter.com`
- Password: `123`

## Architecture

### Backend (.NET 10 API)

**Libraries & Frameworks:**
+ Entity Framework Core 10.0
	+ Fluent API
	+ PostgreSQL provider (Npgsql)
	+ CreatedAt and UpdatedAt by default
	+ Pagination support

+ MediatR (CQRS + Notifications)
+ Swagger/OpenAPI Documentation
+ Fluent Validation
+ Authentication & Authorization
	+ JWT Token with Refresh Token
	+ Claims and profiles configuration
	+ Role-based access control (Admin/User)
	+ Secure password hashing (SHA-512)

+ Serilog (Structured Logging)
	+ Elasticsearch integration
	+ Correlation ID tracking
	
+ OpenAI API Integration
+ HtmlAgilityPack (Web Scraping)
+ PdfPig (PDF Text Extraction)
+ Bogus (Test Data Generation)

**Patterns & Best Practices:**
1. Business Exceptions
2. Exception Middleware
3. Repository Pattern
4. Unit Of Work
5. Base Repository
6. CQRS + Mediator Pattern
7. Notification Pattern
8. Background Services (Hosted Services)
9. Environments configuration
10. CORS Configuration
11. API Versioning (x-api-version header)
12. Globalization/Localization
13. Real database on CI Tests
14. Docker and Docker-Compose

### Frontend (Angular 17)

**Technology:**
+ Angular 17 with Standalone Components
+ Angular Material Design
+ Reactive Forms
+ RxJS for reactive programming
+ TypeScript
+ SCSS styling
+ Lazy loading routes
+ JWT Authentication with auto-refresh

**Features:**
+ Drag & drop CV upload (ngx-file-drop)
+ Real-time CV analysis with loading animations
+ Match percentage visualization
+ Skills analysis display
+ Job management interface
+ Responsive design (mobile-first)

## Docker Services

The application runs 5 containerized services:

| Service | Port | Technology | Purpose |
|---------|------|------------|---------|
| **frontend** | 4200 | Angular 17 + Nginx | Web UI |
| **api** | 5010 | .NET 10 | REST API |
| **postgresql** | 5432 | PostgreSQL 16.3 | Database |
| **elasticsearch** | 9200 | Elasticsearch 8.17.4 | Log storage |
| **kibana** | 5601 | Kibana 8.10.2 | Log viewer |

### Resource Requirements
- **RAM**: 3-4 GB minimum
- **Disk**: 3 GB for images + data
- **CPU**: 2+ cores recommended

## API Endpoints

### Authentication
```http
POST /api/auth/login
POST /api/auth/refresh-token
```

### Jobs
```http
GET  /api/jobs/active
GET  /api/jobs/active?companyCode={code}
POST /api/jobs/sync  # Admin only
```

### Candidates
```http
POST /api/candidates/analyze  # Admin only
Content-Type: multipart/form-data
File: PDF (max 10MB)
```

## Configuration

### Required Settings (appsettings.json)

```json
{
  "ConnectionStrings": {
    "PostgreSql": "Host=localhost;Port=5432;Database=HunterDev;Username=postgres;Password=yourpassword"
  },
  "JwtOptions": {
    "SymmetricSecurityKey": "your-secret-key-here",
    "Issuer": "Hunter API",
    "Audience": "http://www.hunter-api.com",
    "TokenExpirationMinutes": 120
  },
  "OpenAI": {
    "ApiKey": "sk-your-openai-api-key"
  },
  "JobScraper": {
    "CareersUrl": "https://taller.applytojob.com/apply/"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromEmail": "your@email.com",
    "ToEmail": "recipient@email.com",
    "Password": "your-app-password",
    "EnableSsl": true
  },
  "JobSync": {
    "StartTime": "08:00",
    "EndTime": "18:00",
    "IntervalHours": 1,
    "Enabled": true
  }
}
```

## Background Services

**Automatic Job Synchronization:**
- Runs every hour from 8:00 AM to 6:00 PM (Monday-Friday)
- Scrapes job positions from external careers page
- Detects new and closed positions
- Sends email notifications on changes
- Logs all activities to Elasticsearch

**Daily Schedule Example:**
```
08:00 AM ? Sync #1
09:00 AM ? Sync #2
...
06:00 PM ? Sync #11 (last of the day)
```

## Email Notifications

Automatically sends notifications when:
- ? New job positions are added
- ? Job positions are closed/removed

**Email Format:**
```
???????????????????????????????????????
     HUNTER JOB CHANGES DETECTED
???????????????????????????????????????

??? NEW POSITIONS OPENED ???
Total: 2

  ? Senior Backend Developer
  ? Frontend Developer

??? POSITIONS CLOSED ???
Total: 1

  ? Junior Developer
```

## Development Setup

### Prerequisites
- .NET 10 SDK
- Node.js 18+
- PostgreSQL 12+
- Docker Desktop (for containerized setup)
- OpenAI API key

### Option 1: Docker Compose (Recommended)

```bash
# Clone repository
git clone https://github.com/vanderlan/Hunter.git
cd Hunter

# Start all services
docker-compose up --build

# Access at http://localhost:4200
```

### Option 2: Local Development

**Backend:**
```bash
cd src/GeminiCode.Hunter.Api

# Update appsettings.json with your configuration

# Run migrations
dotnet ef database update -p ../GeminiCode.Hunter.Infra.Data

# Start API
dotnet run
```

**Frontend:**
```bash
cd front-end

# Install dependencies
npm install

# Start dev server
npm start

# Access at http://localhost:4200
```

## Database Migrations

```bash
# Navigate to src folder
cd src

# Create new migration
dotnet ef migrations add MigrationName -p GeminiCode.Hunter.Infra.Data -s GeminiCode.Hunter.Api

# Apply migrations
dotnet ef database update -p GeminiCode.Hunter.Infra.Data -s GeminiCode.Hunter.Api --verbose
```

### ?? PostgreSQL DateTime Note
When using PostgreSQL with `timestamp without time zone`, ensure DateTime values use `DateTimeKind.Unspecified`:

```csharp
// ? Correct
var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

// ? Wrong
var now = DateTime.UtcNow; // DateTimeKind.Utc causes errors
```

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## CI/CD

**GitHub Actions:**
1. Unit, Integration and API Tests
2. Continuous Integration
3. Continuous Delivery
4. DockerHub Integration
5. SonarCloud Integration

## Project Structure

```
Hunter/
??? src/
?   ??? GeminiCode.Hunter.Api/              # Web API
?   ??? GeminiCode.Hunter.Application.Core/ # Business Logic
?   ??? GeminiCode.Hunter.Domain.Core/      # Domain Models
?   ??? GeminiCode.Hunter.Infra.Data/       # Data Access
?   ??? GeminiCode.Hunter.Crosscutting.Ioc/ # DI Configuration
?   ??? GeminiCode.Hunter.Crosscutting.Resources/ # Localization
??? tests/
?   ??? GeminiCode.Hunter.Test/             # Unit & Integration Tests
??? front-end/                               # Angular Frontend
?   ??? src/
?   ?   ??? app/
?   ?   ?   ??? components/                 # UI Components
?   ?   ?   ??? services/                   # API Services
?   ?   ?   ??? guards/                     # Route Guards
?   ?   ?   ??? interceptors/               # HTTP Interceptors
?   ?   ?   ??? models/                     # TypeScript Models
?   ?   ??? environments/                   # Environment Config
?   ??? Dockerfile                          # Frontend Docker
?   ??? nginx.conf                          # Nginx Configuration
??? docker-compose.yml                       # Full Stack Orchestration
??? Dockerfile                              # Backend Docker
```

## Docker Commands

### Start Services
```bash
docker-compose up -d              # Start in background
docker-compose up --build         # Rebuild and start
```

### Stop Services
```bash
docker-compose down               # Stop all services
docker-compose down -v            # Stop and remove volumes (?? deletes data)
```

### View Logs
```bash
docker-compose logs -f            # All services
docker-compose logs -f api        # API only
docker-compose logs -f frontend   # Frontend only
```

### Restart Services
```bash
docker-compose restart
docker-compose restart api        # Restart specific service
```

## Monitoring & Logs

**Kibana Dashboard:**
1. Access http://localhost:5601
2. Create index pattern: `hunter-api-*`
3. View structured logs in Discover
4. Create dashboards for monitoring

**Log Correlation:**
- All logs include correlation IDs
- Track requests across services
- Structured JSON logging

## Production Deployment

### Environment Variables
```bash
# API Configuration
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__PostgreSql=Host=prod-db;...
OpenAI__ApiKey=sk-prod-key
Email__Password=app-password

# Database
POSTGRES_PASSWORD=strong-password
```

### SSL/TLS
Add reverse proxy (Nginx/Traefik) with SSL certificates.

### Scaling
```yaml
# docker-compose.prod.yml
services:
  api:
    deploy:
      replicas: 3
```

## Security

- ? JWT authentication with refresh tokens
- ? Role-based authorization
- ? CORS configuration
- ? Password hashing (SHA-512)
- ? SQL injection prevention (EF Core)
- ? XSS protection
- ? CSRF protection
- ? Rate limiting (recommended for production)

## Performance

**CV Analysis Cache:**
- Response caching for 30 minutes
- Content-based cache keys
- Reduces OpenAI API costs by ~95%

**Frontend Optimization:**
- Production build: 676 KB ? 141 KB (gzipped)
- Lazy loading routes
- Code splitting
- Asset caching

## Template Usage

Create your own project based on Hunter:

```bash
dotnet new install .
dotnet new hunter-api --companyName "MyCompany" --output "MyProject" --systemDatabase "PostgreSql"
```

## Default Credentials

?? **Change these in production!**

**Application:**
- Email: `adm@hunter.com`
- Password: `123`

**Database (Docker):**
- User: `postgres`
- Password: `123PgSql`
- Database: `HunterDev`

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License.

## Support

For issues and questions:
- GitHub Issues: https://github.com/vanderlan/Hunter/issues
- Documentation: See `/docs` folder

## Roadmap

- [ ] Implement distributed caching (Redis)
- [ ] Add WebSocket support for real-time updates
- [ ] Multi-language CV support
- [ ] Resume builder feature
- [ ] Interview scheduling
- [ ] Candidate database
- [ ] Advanced analytics dashboard
- [ ] Mobile app (React Native)

*Owner:* https://github.com/vanderlan
