# **Hunter**

[![Build and Test](https://github.com/vanderlan/Hunter/actions/workflows/build.yml/badge.svg)](https://github.com/vanderlan/Hunter/actions/workflows/build.yml)
[![Deploy](https://github.com/vanderlan/Hunter/actions/workflows/deploy.yml/badge.svg)](https://github.com/vanderlan/Hunter/actions/workflows/deploy.yml)

## About this Project

**Hunter** is a comprehensive AI-powered recruitment management system that streamlines the entire hiring process from CV analysis to candidate tracking. It intelligently matches candidates with job positions using advanced AI technology (OpenAI GPT-4o or DeepSeek) and provides complete hiring process management capabilities.

### Key Features

+ **AI-Powered CV Analysis**
	+ Dual AI provider support: OpenAI GPT-4o or DeepSeek (90% cost savings)
	+ PDF text extraction and analysis (PdfPig)
	+ Intelligent match percentage calculation (>70% threshold)
	+ Comprehensive skills gap analysis
	+ Matching and missing skills identification
	+ Detailed recommendations for each candidate

+ **Hiring Process Management**
	+ Complete candidate lifecycle tracking (Referred → Screening → Technical Interview → Client Interview → Hired/Reproved)
	+ Visual timeline of status history
	+ Collaborative notes system for team communication
	+ Role-based status updates (Admin only)
	+ Dashboard with hiring statistics
	+ Integration with CV analyzer for seamless workflow

+ **Job Management & AI Enrichment**
	+ Automated web scraping from external job boards
	+ AI-powered job data extraction and enrichment
	+ Background service for hourly synchronization (8 AM - 6 PM, Mon-Fri)
	+ Email notifications on job changes
	+ Job position tracking and history
	+ Smart sync logic (one-time AI enrichment per job)

+ **User Management & Security**
	+ Role-based access control (Admin/Partner)
	+ JWT authentication with refresh tokens
	+ Secure password hashing (SHA-512)
	+ User profile management
	+ Admin area for user administration

+ **Modern Tech Stack**
+ .NET 10 Web API backend with CQRS pattern
+ Angular 17 standalone components frontend
+ PostgreSQL 16.3 database
+ New Relic APM for monitoring
+ Docker Compose for full-stack orchestration

## 🚀 Quick Start with Docker

**Start the entire system with one command:**

```bash
docker-compose up -d
```

**Access the application:**
- Frontend: http://localhost:5200
- API: http://localhost:5010
- Swagger: http://localhost:5010/swagger

**Login credentials:**
- Email: `adm@hunter.com`
- Password: `123`

## Architecture

### Backend (.NET 10 API)

**Libraries & Frameworks:**
+ Entity Framework Core 10.0
	+ Fluent API with custom mappings
	+ PostgreSQL provider (Npgsql)
	+ CreatedAt and UpdatedAt by default
	+ Pagination support with filters
	+ Cascade delete configurations

+ MediatR (CQRS + Notifications)
	+ Command/Query separation
	+ Request handlers for all operations
	+ Notification pattern for cross-cutting concerns
	
+ Swagger/OpenAPI Documentation
+ Fluent Validation (Input validation)
+ Authentication & Authorization
	+ JWT Token with Refresh Token
	+ Claims and profiles configuration
	+ Role-based access control (Admin/Partner)
	+ Secure password hashing (SHA-512)

+ Serilog (Structured Logging)
+ Console output
+ Correlation ID tracking
+ Request/response logging
	
+ AI Integration (Dual Provider Support)
	+ OpenAI GPT-4o (default)
	+ DeepSeek (90% cost savings alternative)
	+ Structured JSON responses with schema validation
	+ CV analysis and job enrichment
	
+ Additional Libraries
	+ HtmlAgilityPack (Web Scraping)
	+ PdfPig (PDF Text Extraction)
	+ Bogus (Test Data Generation)

**Domain Entities:**
+ **User** - System users with roles (Admin/Partner)
+ **JobPosition** - Job openings with AI-enriched details
+ **Candidate** - Candidate profiles
+ **CvAnalysis** - CV analysis results
+ **CvAnalysisMatch** - Match results between CV and jobs
+ **HiringProcess** - Complete hiring lifecycle tracking
+ **HiringProcessStatusHistory** - Status change audit trail
+ **HiringProcessNote** - Collaborative notes
+ **RefreshToken** - JWT refresh tokens
+ **CandidateFile** - Uploaded CV files

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
+ Role-based UI rendering

**Components & Features:**
+ **CV Analyzer**
	+ Drag & drop CV upload (ngx-file-drop)
	+ Real-time CV analysis with loading animations
	+ Match percentage visualization with cards
	+ Skills analysis display (matching & missing)
	+ Job details dialog
	+ Start hiring process from matches
	
+ **Hiring Process Management**
	+ Dashboard with statistics (total, active, hired, reproved)
	+ Paginated list with search functionality
	+ Color-coded status chips
	+ Detailed view with visual timeline
	+ Collaborative notes section
	+ Status update form (Admin only)
	
+ **Job Positions Management**
	+ List all job positions
	+ Create, update, and manage jobs
	+ Job details with AI-enriched content
	+ Skills visualization
	
+ **Admin Area**
	+ User management (create, update, delete)
	+ Role assignment (Admin/Partner)
	+ User activity monitoring
	
+ **Additional Features**
	+ Responsive design (mobile-first)
	+ Change password functionality
	+ User preferences management
	+ Login with remember me
	+ Sidebar navigation with role-based menu items

## Docker Services

The application runs 3 containerized services:

| Service | Port | Technology | Purpose |
|---------|------|------------|---------|
| **frontend** | 5200 | Angular 17 + Nginx | Web UI |
| **api** | 5010 | .NET 10 + New Relic | REST API with APM |
| **postgresql** | 5432 | PostgreSQL 16.3 | Database |

### Resource Requirements
- **RAM**: 2 GB minimum
- **Disk**: 1 GB for images + data
- **CPU**: 2+ cores recommended

### New Relic Integration
The API service includes the New Relic .NET agent for application performance monitoring. The agent is automatically installed and configured in the Docker container with:
- Automatic instrumentation of ASP.NET Core
- Database query monitoring
- External service call tracking
- Error tracking and alerting
- Custom transaction naming

## API Endpoints

### Authentication
```http
POST /api/auth/login          # User login
POST /api/auth/refresh-token  # Refresh JWT token
```

### Job Positions
```http
GET  /api/JobPositions                      # List all jobs (paginated)
GET  /api/JobPositions/{id}                 # Get job by ID
POST /api/JobPositions                      # Create new job (Admin)
PUT  /api/JobPositions/{id}                 # Update job (Admin)
DELETE /api/JobPositions/{id}               # Delete job (Admin)
POST /api/JobPositions/Sync                 # Sync from external source (Admin)
GET  /api/JobPositions/active               # List active jobs
GET  /api/JobPositions/active?companyCode=X # Filter by company
```

### CV Analysis
```http
POST /api/CvAnalysis/Analyze  # Analyze CV against jobs
Content-Type: multipart/form-data
File: PDF (max 10MB)
Body: { candidateName, candidateEmail }
```

### Hiring Process
```http
GET  /api/HiringProcess              # List all hiring processes (paginated)
GET  /api/HiringProcess/{id}         # Get process details
GET  /api/HiringProcess/Dashboard    # Get dashboard statistics
POST /api/HiringProcess              # Start new process (Admin/Partner)
PUT  /api/HiringProcess/{id}/Status  # Update status (Admin only)
POST /api/HiringProcess/{id}/Notes   # Add note (Admin/Partner)
```

### User Management
```http
GET  /api/Users           # List all users (Admin)
GET  /api/Users/{id}      # Get user by ID (Admin)
POST /api/Users           # Create user (Admin)
PUT  /api/Users/{id}      # Update user (Admin)
DELETE /api/Users/{id}    # Delete user (Admin)
PUT  /api/Users/Password  # Change own password
```

### Candidates
```http
POST /api/Candidates/Analyze  # Deprecated: Use /api/CvAnalysis/Analyze
# This endpoint is maintained for backward compatibility
# Consider migrating to the new endpoint
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
  "AI": {
    "Provider": "OpenAI"  // or "DeepSeek" for 90% cost savings
  },
  "OpenAI": {
    "ApiKey": "sk-your-openai-api-key",
    "Model": "gpt-4o"
  },
  "DeepSeek": {
    "ApiKey": "sk-your-deepseek-api-key",  // Required when using DeepSeek provider
    "Model": "deepseek-chat"
  },
  "JobScraper": {
    "CareersUrl": "https://taller.applytojob.com/apply/"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromEmail": "your@email.com",
    "ToEmails": ["recipient@email.com"],  // Array of recipients (or use "ToEmail" for single)
    "Password": "your-app-password",
    "EnableSsl": true,
    "ApplicationUrl": "http://localhost:5200"
  },
  "JobSync": {
    "StartTime": "08:00",
    "EndTime": "18:00",
    "IntervalHours": 1,
    "Enabled": true
  },
  "NewRelic": {
    "AppName": "Hunter",
    "LicenseKey": "your-new-relic-license-key"
  }
}
```

### New Relic Configuration

New Relic APM is integrated for application performance monitoring. The configuration is managed through:

**appsettings.json:**
```json
{
  "NewRelic": {
    "AppName": "Hunter",
    "LicenseKey": "your-new-relic-license-key"
  }
}
```

**Environment Variables (Docker/Production):**
```bash
NEW_RELIC_LICENSE_KEY=your-license-key
NEW_RELIC_APP_NAME=Hunter
CORECLR_ENABLE_PROFILING=1
CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A}
CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent
CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so
```

The New Relic agent is automatically installed in the Docker container and will start monitoring as soon as the application starts.

## AI Provider Configuration

**OpenAI (Default - High Quality)**
```json
{
  "AI": {
    "Provider": "OpenAI"  // or omit this setting
  },
  "OpenAI": {
    "ApiKey": "sk-your-key",
    "Model": "gpt-4o"
  }
}
```

**DeepSeek (90% Cost Savings)**
```json
{
  "AI": {
    "Provider": "DeepSeek"
  },
  "DeepSeek": {
    "ApiKey": "sk-your-key",
    "Model": "deepseek-chat"
  }
}
```

**Cost Comparison (Pricing as of January 2026):**

| Provider | Cost per 1M tokens | Typical Monthly Cost |
|----------|-------------------|---------------------|
| OpenAI GPT-4o | $2.50 / $10.00 (in/out) | ~$15.60 |
| DeepSeek | $0.27 / $1.10 (in/out) | ~$1.56 |

*Note: AI provider pricing may change. Please check provider websites for current rates.*

## AI Job Enrichment

The Hunter uses AI to automatically extract and structure job information from scraped web pages, providing high-quality, consistent data for CV matching.

### How It Works

1. **Job Scraper** fetches raw HTML from external job boards
2. **AI Service** (OpenAI or DeepSeek) analyzes the HTML content
3. **Structured Extraction** generates:
   - Job summary
   - Detailed responsibilities
   - Comprehensive skills list
   - Must-have skills (required)
   - Nice-to-have skills (optional)
4. **One-time Processing** - Each job is enriched only once (cost optimization)
5. **Existing Jobs** - Only update LastCheckedAt timestamp (no AI call)

### Benefits

✅ **Accuracy** - AI understands context better than regex/HTML parsing  
✅ **Maintainability** - No brittle HTML selectors to maintain  
✅ **Consistency** - Structured JSON output every time  
✅ **Cost-Effective** - One-time processing per new job  
✅ **Flexible** - Works with any HTML structure  

### Smart Sync Logic

```
NEW Job → Extract HTML → Call AI → Save Enriched Data
EXISTING Job (same title) → Update LastCheckedAt only (no AI call)
CHANGED Title → Mark as closed + Create as new job
```

**Monthly Cost Estimate (50 new jobs):**
- Input tokens: 50 × 3000 = 150K tokens
- Output tokens: 50 × 400 = 20K tokens
- **OpenAI:** ~$0.58/month
- **DeepSeek:** ~$0.06/month

## Background Services

**Automatic Job Synchronization:**
- Runs every hour from 8:00 AM to 6:00 PM (Monday-Friday)
- Scrapes job positions from external careers page
- Detects new and closed positions
- Sends email notifications on changes
- Logs all activities to Elasticsearch

**Daily Schedule Example:**
```
08:00 AM → Sync #1
09:00 AM → Sync #2
...
06:00 PM → Sync #11 (last of the day)
```

## Email Notifications

Automatically sends notifications when:
- 📩 New job positions are added
- 📩 Job positions are closed/removed

**Email Format:**
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
     HUNTER JOB CHANGES DETECTED
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

✅ NEW POSITIONS OPENED ✅
Total: 2

  ◉ Senior Backend Developer
  ◉ Frontend Developer

❌ POSITIONS CLOSED ❌
Total: 1

  ◉ Junior Developer
```

## Hiring Process Workflow

The Hunter provides a complete hiring process management system to track candidates from initial referral to final decision.

### Status Flow

```
1. Referred (Initial Status)
   ↓
2. Screening
   ↓
3. Technical Interview
   ↓
4. Client Interview
   ↓
5. Hired ✅  or  Reproved ❌
```

### Features

**For Partners:**
- Start hiring processes from CV matches
- View all hiring processes
- Add collaborative notes
- Track candidate progress

**For Admins:**
- All partner capabilities
- Update process status
- Receive notifications for new processes
- Manage complete hiring lifecycle

### Integration

The hiring process integrates seamlessly with the CV analyzer:
1. Upload CV and analyze against job positions
2. View match results with percentages
3. Click "Start Hiring Process" on promising matches
4. Track candidate through all stages
5. Collaborate with team via notes
6. Update status as candidate progresses

### Dashboard Statistics

- Total processes
- Active processes
- Hired count
- Reproved count
- Success rate calculation

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

### ⚠️ PostgreSQL DateTime Note
When using PostgreSQL with `timestamp without time zone`, ensure DateTime values use `DateTimeKind.Unspecified`:

```csharp
// ✅ Correct
var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

// ❌ Wrong
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
├── src/
│   ├── GeminiCode.Hunter.Api/                      # Web API Layer
│   │   ├── Controllers/                            # API Controllers
│   │   │   ├── V1/AuthController.cs                # Authentication
│   │   │   ├── V1/JobPositionsController.cs        # Job Management
│   │   │   ├── V1/CvAnalysisController.cs          # CV Analysis
│   │   │   ├── V1/HiringProcessController.cs       # Hiring Process
│   │   │   ├── V1/UsersController.cs               # User Management
│   │   │   └── V1/CandidatesController.cs          # Candidates (legacy)
│   │   └── Middlewares/                            # Exception handling, CORS
│   │
│   ├── GeminiCode.Hunter.Application.Core/         # Business Logic (CQRS)
│   │   ├── UseCases/                               # Commands & Queries
│   │   │   ├── Auth/                               # Login, Refresh Token
│   │   │   ├── JobPositions/                       # CRUD, Sync
│   │   │   ├── CvAnalysis/                         # Analyze CV
│   │   │   ├── HiringProcess/                      # Process Management
│   │   │   ├── Users/                              # User Management
│   │   │   └── Candidates/                         # Candidate matching
│   │   ├── Services/                               # Application Services
│   │   │   ├── OpenAIService.cs                    # AI Integration (dual provider)
│   │   │   ├── JobScraperService.cs                # Web scraping
│   │   │   ├── EmailService.cs                     # Email notifications
│   │   │   └── BackgroundServices/                 # Hosted Services
│   │   └── Notifications/                          # MediatR notifications
│   │
│   ├── GeminiCode.Hunter.Domain.Core/              # Domain Models
│   │   ├── Entities/                               # Domain Entities
│   │   │   ├── User.cs                             # Users (Admin/Partner)
│   │   │   ├── JobPosition.cs                      # Job openings
│   │   │   ├── Candidate.cs                        # Candidates
│   │   │   ├── CvAnalysis.cs                       # Analysis results
│   │   │   ├── CvAnalysisMatch.cs                  # CV-Job matches
│   │   │   ├── HiringProcess.cs                    # Hiring lifecycle
│   │   │   ├── HiringProcessStatusHistory.cs       # Status audit trail
│   │   │   ├── HiringProcessNote.cs                # Collaborative notes
│   │   │   ├── CandidateFile.cs                    # Uploaded CVs
│   │   │   └── RefreshToken.cs                     # JWT refresh tokens
│   │   ├── Models/                                 # DTOs, Requests, Responses
│   │   └── Services/                               # Domain service interfaces
│   │
│   ├── GeminiCode.Hunter.Infra.Data/               # Data Access Layer
│   │   ├── Context/                                # EF Core DbContext
│   │   ├── Mapping/                                # Fluent API mappings
│   │   ├── Repositories/                           # Repository implementations
│   │   ├── UnitOfWork/                             # Unit of Work pattern
│   │   └── Migrations/                             # EF Core migrations
│   │
│   ├── GeminiCode.Hunter.Crosscutting.Ioc/         # Dependency Injection
│   └── GeminiCode.Hunter.Crosscutting.Resources/   # Localization Resources
│
├── tests/
│   └── GeminiCode.Hunter.Test/                     # Unit & Integration Tests
│       ├── Unit/                                   # Unit tests
│       ├── Integration/                            # Integration tests
│       └── Shared/Faker/                           # Test data generators
│
├── front-end/                                       # Angular 17 Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/                         # UI Components
│   │   │   │   ├── login/                          # Login page
│   │   │   │   ├── cv-analyzer/                    # CV analysis UI
│   │   │   │   ├── hiring-process/                 # Process list view
│   │   │   │   ├── hiring-process-detail/          # Process detail view
│   │   │   │   ├── job-positions/                  # Job management
│   │   │   │   ├── user-management/                # User admin
│   │   │   │   ├── admin/                          # Admin dashboard
│   │   │   │   ├── match-result-card/              # Match display
│   │   │   │   ├── resume-viewer/                  # PDF viewer
│   │   │   │   ├── change-password/                # Password change
│   │   │   │   ├── preferences/                    # User preferences
│   │   │   │   └── candidates/                     # Candidate list
│   │   │   ├── services/                           # API Services
│   │   │   │   ├── auth.service.ts                 # Authentication
│   │   │   │   ├── cv-analysis.service.ts          # CV analysis
│   │   │   │   ├── hiring-process.service.ts       # Hiring process
│   │   │   │   ├── job-position.service.ts         # Job positions
│   │   │   │   └── user.service.ts                 # User management
│   │   │   ├── guards/                             # Route Guards
│   │   │   │   ├── auth.guard.ts                   # Authentication
│   │   │   │   └── role.guard.ts                   # Authorization
│   │   │   ├── interceptors/                       # HTTP Interceptors
│   │   │   │   ├── auth.interceptor.ts             # JWT injection
│   │   │   │   └── error.interceptor.ts            # Error handling
│   │   │   └── models/                             # TypeScript Models
│   │   │       ├── user.model.ts
│   │   │       ├── job.model.ts
│   │   │       ├── cv-analysis.model.ts
│   │   │       └── hiring-process.model.ts
│   │   └── environments/                           # Environment configs
│   ├── Dockerfile                                  # Frontend container
│   └── nginx.conf                                  # Nginx web server config
│
├── .github/
│   └── workflows/                                  # CI/CD Pipelines
│       ├── build.yml                               # Build & Test
│       └── deploy.yml                              # Deploy to production
│
├── docker-compose.yml                              # Development orchestration
├── docker-compose.prod.yml                         # Production orchestration
├── Dockerfile                                      # Backend container
├── deploy.sh                                       # Production deployment script
└── README.md                                       # This file
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
docker-compose down -v            # Stop and remove volumes (⚠️ deletes data)
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

**New Relic APM:**

The Hunter API is integrated with New Relic for comprehensive application performance monitoring (APM). New Relic provides:
- Real-time performance metrics
- Transaction tracing
- Error tracking and alerting
- Database query performance analysis
- Custom instrumentation support

The New Relic agent is automatically installed and configured in the Docker container. The agent will start monitoring your application as soon as it runs.

**New Relic Configuration:**
- License Key: Configured via environment variables
- App Name: "Hunter"
- Agent Version: 10.47.2

To view your application metrics:
1. Log in to your New Relic account at https://one.newrelic.com
2. Navigate to APM & Services
3. Find your "Hunter" application
4. View real-time performance data, errors, and transactions

**Application Logs:**
- Console logging for development
- Structured logging with correlation IDs
- Request/response logging

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

- ✅ JWT authentication with refresh tokens
- ✅ Role-based authorization (Admin/Partner)
- ✅ CORS configuration
- ✅ Password hashing (SHA-512)
- ✅ SQL injection prevention (EF Core)
- ✅ XSS protection
- ✅ CSRF protection
- 🔒 Rate limiting (recommended for production)

## Performance

**CV Analysis Cache:**
- Response caching for 30 minutes
- Content-based cache keys
- Reduces OpenAI API costs by ~95%

**Frontend Optimization:**
- Production build: 676 KB → 141 KB (gzipped)
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

⚠️ **Change these in production!**

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

### ✅ Completed Features
- [x] AI-powered CV analysis with OpenAI GPT-4o
- [x] Dual AI provider support (OpenAI/DeepSeek)
- [x] Hiring process management with status tracking
- [x] Automated job scraping and AI enrichment
- [x] Background job synchronization service
- [x] Email notifications for job changes
- [x] User management with role-based access
- [x] JWT authentication with refresh tokens
- [x] Angular 17 frontend with Material Design
- [x] Docker containerization
- [x] CI/CD with GitHub Actions
- [x] Elasticsearch + Kibana logging
- [x] Collaborative notes system
- [x] Status history audit trail

### 🚀 Planned Features
- [ ] Implement distributed caching (Redis)
- [ ] Add WebSocket support for real-time updates
- [ ] Multi-language CV support (Spanish, French, etc.)
- [ ] Resume builder feature
- [ ] Interview scheduling integration
- [ ] Advanced candidate database with search
- [ ] Analytics dashboard with charts and metrics
- [ ] Mobile app (React Native)
- [ ] Bulk CV upload and analysis
- [ ] Email templates customization
- [ ] Integration with LinkedIn API
- [ ] Automated interview question generation
- [ ] Video interview integration
- [ ] Candidate skill assessments
- [ ] Offer letter generation

*Owner:* https://github.com/vanderlan
