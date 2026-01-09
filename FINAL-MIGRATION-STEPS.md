# Final Changes Required - Requirements to Skills Migration

## Remaining Files to Update

### 1. JobPositionSummary.cs (OpenAI Model)
**File:** `src/GeminiCode.Hunter.Domain.Core/Models/External/OpenAI/JobPositionSummary.cs`

**Change:**
```csharp
// REMOVE Requirements parameter
public record JobPositionSummary(
  string JobCode,
  string Title,
  string CompanyCode,
  string? Summary,
  string? Responsibilities,
  string? MustHaveSkills,
  string? NiceToHaveSkills
);
```

Note: Requirements not needed for CV matching - AI uses Summary, Responsibilities, and Skills fields

### 2. AnalyzeCvRequestHandler.cs (Line 84)
**File:** `src/GeminiCode.Hunter.Application.Core/UseCases/Candidates/Commands/AnalyzeCv/AnalyzeCvRequestHandler.cs`

**Change Line 84:**
```csharp
// OLD:
var jobSummaries = jobPositions.Select(j => new JobPositionSummary(
    j.JobCode,
    j.Title,
    j.CompanyCode,
    j.Summary,
    j.Responsibilities,
    j.Requirements,  // ? REMOVE THIS LINE
    j.MustHaveSkills,
    j.NiceToHaveSkills
)).ToList();

// NEW:
var jobSummaries = jobPositions.Select(j => new JobPositionSummary(
    j.JobCode,
    j.Title,
    j.CompanyCode,
    j.Summary,
    j.Responsibilities,
    j.MustHaveSkills,
    j.NiceToHaveSkills
)).ToList();
```

### 3. CandidateMatchingNotificationHandler.cs (Line 78)
**File:** `src/GeminiCode.Hunter.Application.Core/UseCases/Candidates/Notifications/CandidateMatching/CandidateMatchingNotificationHandler.cs`

**Find Line 78:**
```csharp
// Change from:
position.Requirements,

// To: (remove this parameter entirely, or if needed):
position.Skills,  // if JobPositionSummary uses it
// OR just remove it if not needed
```

### 4. JobPositionFaker.cs (Test Faker)
**File:** `tests/GeminiCode.Hunter.Test/Shared/Faker/JobPositionFaker.cs`

**Change Line 20:**
```csharp
// OLD:
.RuleFor(o => o.Requirements, f => f.Lorem.Paragraphs(2))

// NEW:
.RuleFor(o => o.Skills, f => string.Join(", ", f.Make(10, () => f.Company.Bs())))
```

This generates comma-separated skills like: "synergize, innovate, optimize, ..."

### 5. Job Positions Component HTML (Frontend - Optional)
**File:** `front-end/src/app/components/job-positions/job-positions.component.html`

The form still has a "Requirements" field. You may want to either:
- **Keep it** (for manual job entry) and map to Skills on backend
- **Replace with Skills** field

**Option A: Keep Requirements** (Map on Backend)
```html
<!-- In form, keep as is but rename label: -->
<mat-form-field appearance="outline" class="full-width">
  <mat-label>Skills</mat-label>
  <textarea matInput formControlName="requirements" rows="3" placeholder="Comma-separated skills: Java, C#, SQL..."></textarea>
</mat-form-field>
```

Then in backend create/update handlers, map `request.Requirements` ? `jobPosition.Skills`

**Option B: Replace with Skills** (Consistent naming)
```typescript
// In component.ts, change FormGroup:
this.jobPositionForm = this.fb.group({
  title: ['', Validators.required],
  companyCode: ['', Validators.required],
  jobCode: ['', Validators.required],
  externalUrl: ['', Validators.required],
  summary: [''],
  responsibilities: [''],
  skills: [''],  // ? Changed from requirements
  mustHaveSkills: [''],
  niceToHaveSkills: [''],
  isActive: [true]
});
```

```html
<!-- In HTML: -->
<mat-form-field appearance="outline" class="full-width">
  <mat-label>Skills</mat-label>
  <textarea matInput formControlName="skills" rows="3" placeholder="Comma-separated: Java, C#, SQL..."></textarea>
</mat-form-field>
```

---

## Database Migration

After all code changes, create and run migration:

```bash
cd src

# Create migration
dotnet ef migrations add ReplaceRequirementsWithSkills \
  -p GeminiCode.Hunter.Infra.Data \
  -s GeminiCode.Hunter.Api

# Apply migration
dotnet ef database update \
  -p GeminiCode.Hunter.Infra.Data \
  -s GeminiCode.Hunter.Api --verbose
```

Expected migration SQL:
```sql
ALTER TABLE "JobPosition" RENAME COLUMN "Requirements" TO "Skills";
```

---

## Quick Fix Script (All Backend Changes)

### 1. Update JobPositionSummary.cs
```csharp
namespace GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

public record JobPositionSummary(
  string JobCode,
  string Title,
  string CompanyCode,
  string? Summary,
  string? Responsibilities,
  string? MustHaveSkills,
  string? NiceToHaveSkills
);
```

### 2. Update AnalyzeCvRequestHandler.cs (around line 78-88)
```csharp
var jobSummaries = jobPositions.Select(j => new JobPositionSummary(
    j.JobCode,
    j.Title,
    j.CompanyCode,
    j.Summary,
    j.Responsibilities,
    j.MustHaveSkills,
    j.NiceToHaveSkills
)).ToList();
```

### 3. Find and fix CandidateMatchingNotificationHandler.cs
Search for `position.Requirements` and remove or replace with `position.Skills`

### 4. Update JobPositionFaker.cs
```csharp
.RuleFor(o => o.Skills, f => string.Join(", ", f.Make(10, () => f.Company.Bs())))
```

---

## Build and Test

```bash
# Backend
cd src/GeminiCode.Hunter.Api
dotnet build

# Should succeed with no errors

# Frontend
cd front-end
npm run build

# Should succeed

# Run migration
cd ../src
dotnet ef database update -p GeminiCode.Hunter.Infra.Data -s GeminiCode.Hunter.Api

# Test
dotnet test
```

---

## Summary of What Was Changed

### ? Completed:
1. Entity: `JobPosition` - removed Requirements, added Skills
2. Mapping: `JobPositionMapping` - updated schema
3. Services: `OpenAIService` - added EnrichJobDetailsAsync
4. Services: `JobScraperService` - simplified to raw HTML only
5. Handlers: `SyncJobsRequestHandler` - uses AI enrichment, avoids re-processing
6. DTOs: Create/Update/GetById requests/responses
7. Models: `ScrapedJobDetails`, `JobEnrichmentResult`
8. Frontend: `job.model.ts`, `position-details-dialog.component.ts`

### ?? Remaining (4 files):
1. `JobPositionSummary.cs` - remove Requirements parameter
2. `AnalyzeCvRequestHandler.cs` - fix line 84
3. `CandidateMatchingNotificationHandler.cs` - fix line 78
4. `JobPositionFaker.cs` - fix line 20

---

## Testing Checklist

After all changes:

- [ ] Backend builds successfully
- [ ] Frontend builds successfully
- [ ] Migration runs successfully
- [ ] Job sync creates new positions with AI-enriched data
- [ ] CV analysis works with updated job summaries
- [ ] Job positions CRUD works (create/update with Skills)
- [ ] Position details dialog shows Skills section
- [ ] No OpenAI calls for existing jobs (only LastCheckedAt updates)

---

**Status:** 95% Complete  
**Remaining:** 4 files (10 minutes of work)  
**Benefits:** Better data extraction, lower costs, more accurate matching
