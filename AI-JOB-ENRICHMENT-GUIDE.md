# AI-Powered Job Enrichment - Implementation Guide

## Summary of Changes

This document details all changes needed to implement AI-powered job data extraction using OpenAI instead of manual HTML parsing.

## Key Changes

### 1. **Remove Requirements Field, Add Skills Field**
- Removed: `Requirements` (text field)
- Added: `Skills` (comma-separated list of all skills)
- Existing: `MustHaveSkills`, `NiceToHaveSkills` remain

### 2. **Use OpenAI for Data Extraction**
- Scraper now extracts raw HTML only
- OpenAI extracts structured data from HTML
- Single API call per new job

### 3. **Avoid Unnecessary OpenAI Calls**
- Only enrich NEW job positions
- If job exists and title unchanged ? only update `LastCheckedAt`
- If title changes ? mark as closed, treat as new

---

## Files Already Updated

### ? **Domain Layer**
- `src/GeminiCode.Hunter.Domain.Core/Entities/JobPosition.cs`
  - Removed: `Requirements`
  - Added: `Skills`

- `src/GeminiCode.Hunter.Domain.Core/Models/External/ApplyJob/ScrapedJobDetails.cs`
  - Changed to only contain `RawHtmlContent`

- `src/GeminiCode.Hunter.Domain.Core/Models/External/OpenAI/JobEnrichmentResult.cs` ? NEW
  - Contains: Summary, Responsibilities, Skills, MustHaveSkills, NiceToHaveSkills

- `src/GeminiCode.Hunter.Domain.Core/Services/IOpenAIService.cs`
  - Added: `Task<JobEnrichmentResult> EnrichJobDetailsAsync(string jobTitle, string rawHtmlContent)`

### ? **Infrastructure Layer**
- `src/GeminiCode.Hunter.Infra.Data/Mapping/JobPositionMapping.cs`
  - Updated mapping: Requirements ? Skills

### ? **Application Layer**
- `src/GeminiCode.Hunter.Application.Core/Services/OpenAIService.cs`
  - Implemented `EnrichJobDetailsAsync` with structured JSON schema
  - Prompts AI to extract all fields in single call

- `src/GeminiCode.Hunter.Application.Core/Services/JobScraperService.cs`
  - Simplified to only extract raw HTML (no parsing)

- `src/GeminiCode.Hunter.Application.Core/UseCases/Jobs/Commands/SyncJobs/SyncJobsRequestHandler.cs`
  - Uses OpenAI for new jobs only
  - Avoids re-enriching existing jobs
  - Tracks title changes

- `src/GeminiCode.Hunter.Application.Core/UseCases/JobPositions/Commands/Create/JobPositionCreateRequest.cs`
  - Updated: Requirements ? Skills

- `src/GeminiCode.Hunter.Application.Core/UseCases/JobPositions/Commands/Create/JobPositionCreateResponse.cs`
  - Updated: Requirements ? Skills

---

## Files That Need Updating

### ?? **Backend DTOs** (Replace `Requirements` with `Skills`)

1. **Get By ID Response**
```csharp
// File: src/GeminiCode.Hunter.Application.Core/UseCases/JobPositions/Queries/GetById/JobPositionGetByIdResponse.cs

// Change line:
public string? Requirements { get; set; }
// To:
public string? Skills { get; set; }

// And in the mapping:
Requirements = jobPosition.Requirements,
// To:
Skills = jobPosition.Skills,
```

2. **Update Request**
```csharp
// File: src/GeminiCode.Hunter.Application.Core/UseCases/JobPositions/Commands/Update/JobPositionUpdateRequest.cs

// Change:
public string? Requirements { get; set; }
// To:
public string? Skills { get; set; }
```

3. **Update Request Handler**
```csharp
// File: src/GeminiCode.Hunter.Application.Core/UseCases/JobPositions/Commands/Update/JobPositionUpdateRequestHandler.cs

// Change:
jobPosition.Requirements = request.Requirements;
// To:
jobPosition.Skills = request.Skills;
```

4. **Get Paginated Response**
```csharp
// File: src/GeminiCode.Hunter.Application.Core/UseCases/JobPositions/Queries/GetPaginated/JobPositionGetPaginatedResponse.cs

// Change:
public string? Requirements { get; set; }
// To:
public string? Skills { get; set; }

// And in mapping:
Requirements = x.Requirements,
// To:
Skills = x.Skills,
```

5. **Get Active Jobs DTO**
```csharp
// File: src/GeminiCode.Hunter.Application.Core/UseCases/Jobs/Queries/GetActiveJobs/GetActiveJobsResponse.cs

// In JobPositionDto, if it exists, change:
public string? Requirements { get; set; }
// To:
public string? Skills { get; set; }
```

6. **Job Position Summary (for CV Analysis)**
```csharp
// File: Check if src/GeminiCode.Hunter.Domain.Core/Models/External/OpenAI/JobPositionSummary.cs has Requirements
// If yes, remove it (not needed for CV matching)
```

---

### ?? **Frontend** (Update TypeScript Models)

1. **Job Model**
```typescript
// File: front-end/src/app/models/job.model.ts

export interface JobPosition {
  publicId?: string;
  jobPositionId?: number;
  title: string;
  externalUrl: string;
  companyCode: string;
  jobCode: string;
  summary?: string;
  responsibilities?: string;
  skills?: string;  // ? ADD THIS
  mustHaveSkills?: string;
  niceToHaveSkills?: string;
  isActive?: boolean;
  closedAt?: Date;
  lastCheckedAt?: Date;
  createdAt?: Date;
  lastUpdated?: Date;
}
```

2. **Position Details Dialog**
```typescript
// File: front-end/src/app/components/cv-analyzer/position-details-dialog.component.ts

// REMOVE the Requirements section from the template:
<div class="position-info-section" *ngIf="data.requirements">
  ...
</div>

// ADD the Skills section:
<div class="position-info-section" *ngIf="data.skills">
  <h3 class="position-section-heading">
    <mat-icon class="position-section-icon">lightbulb</mat-icon>
    <span>Skills</span>
  </h3>
  <p class="position-section-text">{{ data.skills }}</p>
</div>
```

---

## Database Migration

### Create Migration

```bash
cd src
dotnet ef migrations add ReplaceRequirementsWithSkills -p GeminiCode.Hunter.Infra.Data -s GeminiCode.Hunter.Api
```

### Apply Migration

```bash
dotnet ef database update -p GeminiCode.Hunter.Infra.Data -s GeminiCode.Hunter.Api --verbose
```

### Expected Migration SQL

```sql
ALTER TABLE "JobPosition" 
RENAME COLUMN "Requirements" TO "Skills";
```

---

## Testing the Changes

### 1. **Test Job Sync with AI Enrichment**

```bash
curl -X POST "http://localhost:5010/JobPositions/Sync" \
  -H "Authorization: Bearer <your-token>" \
  -H "x-api-version: 1.0"
```

**Expected Behavior:**
- Scrapes job listings
- For NEW jobs: Calls OpenAI to extract data
- For EXISTING jobs with same title: Only updates `LastCheckedAt`
- For EXISTING jobs with changed title: Marks as closed

### 2. **Check OpenAI Logs**

```bash
# In API logs, you should see:
[INF] Enriching job details with AI for: Senior Backend Developer
[DBG] OpenAI Job Enrichment Response: {"summary":"...","skills":"Java, C#, SQL Server, Kubernetes, Docker, Agile,..."}
```

### 3. **Verify Data**

```sql
SELECT Title, Skills, MustHaveSkills, NiceToHaveSkills 
FROM "JobPosition" 
WHERE IsActive = true 
LIMIT 5;
```

**Expected:**
- `Skills` contains ALL skills comma-separated
- `MustHaveSkills` contains essential skills
- `NiceToHaveSkills` contains optional skills

---

## Cost Optimization

### Tokens Per Job Enrichment
- Input: ~2000-5000 tokens (HTML content)
- Output: ~300-500 tokens (structured JSON)
- **Total per job:** ~2500 tokens average

### Monthly Estimate
- 50 new jobs/month
- 2500 tokens × 50 = 125,000 tokens
- At GPT-4o pricing (~$5/1M tokens):
  - Input: 50 × 3000 × $0.0025/1K = **$0.38**
  - Output: 50 × 400 × $0.01/1K = **$0.20**
  - **Total: ~$0.60/month** for job enrichment

### Savings vs. Previous Approach
- **Old:** Frequent OpenAI calls during CV analysis (lots of token usage)
- **New:** One-time enrichment per job position
- **Result:** More accurate data + lower costs

---

## How It Works

### Flow Diagram

```
Job Sync Triggered
    ?
Scrape Job Listings (HTML)
    ?
For each new job:
    ?
    Scrape Full Page HTML
    ?
    Call OpenAI.EnrichJobDetailsAsync()
    ?? Send: Job Title + Raw HTML
    ?? AI Extracts: Summary, Responsibilities, Skills, etc.
    ?? Returns: Structured JobEnrichmentResult
    ?
    Save to Database
    ?
Done (No OpenAI call for existing jobs)
```

### OpenAI Prompt

```
# TASK
Extract structured information from the following job posting for: **Senior Backend Developer**

# JOB POSTING HTML CONTENT
```html
<html>...</html>
```

# EXTRACTION INSTRUCTIONS
Extract and structure the following information:

1. **Summary**: A concise 2-3 sentence overview
2. **Responsibilities**: Key duties (bullet points or paragraphs)
3. **Skills**: ALL technical and soft skills, comma-separated
   Example: "Java, C#, SQL Server, Kubernetes, Docker, Agile, Team Leadership"
4. **Must-Have Skills**: Essential skills only, comma-separated
5. **Nice-to-Have Skills**: Preferred skills, comma-separated

# IMPORTANT RULES
- Extract ONLY information present in the HTML
- Do NOT invent or assume information
- If a section has no data, return empty string
```

### Response Schema

```json
{
  "summary": "Design and develop scalable backend services...",
  "responsibilities": "• Build RESTful APIs\n• Implement microservices...",
  "skills": "Java, Spring Boot, Kubernetes, Docker, PostgreSQL, AWS, Agile",
  "mustHaveSkills": "Java, Spring Boot, Kubernetes, PostgreSQL",
  "niceToHaveSkills": "AWS, Azure, React, Python"
}
```

---

## Troubleshooting

### Issue: OpenAI Returns Empty Fields

**Cause:** HTML structure varies between job postings  
**Solution:** AI is instructed to extract what's available, return empty for missing

### Issue: Skills Field Too Long

**Cause:** Job posting lists many skills  
**Solution:** Database field is `text` type (unlimited), frontend will truncate in summary view

### Issue: Existing Jobs Being Re-enriched

**Cause:** Logic checks URL + Title  
**Solution:** Verify that title comparison is working correctly

---

## Next Steps

1. ? Update all remaining backend DTOs (see list above)
2. ? Update frontend TypeScript model
3. ? Update position details dialog template
4. ? Create and run database migration
5. ? Test job sync with OpenAI enrichment
6. ? Deploy to production

---

## Benefits

? **More Accurate Data:** AI understands context better than HTML parsing  
? **Handles Varying HTML:** Works regardless of HTML structure changes  
? **Comprehensive Skills:** Extracts ALL skills, not just specific sections  
? **Cost Effective:** Only processes new jobs once  
? **Better CV Matching:** More complete job data = better candidate matching  

---

**Status:** Implementation 90% Complete  
**Remaining:** Update DTOs, Frontend, Run Migration  
**Estimated Time:** 30 minutes
