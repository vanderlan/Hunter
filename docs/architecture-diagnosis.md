# Hunter — Architectural Diagnosis

**Date:** 2026-11-06
**Branch analysed:** `hfi/ddbcfa26-6907-4ecf-93c1-646140588005/elm` (HEAD `49b28da`)
**Scope:** full stack — .NET 10 backend, Angular 21 front-end, data layer, CI/CD, infrastructure
**Purpose:** establish a factual baseline before the modernization work. This document *diagnoses*; it proposes no changes to the code yet.

---

## 1. Executive Summary

Hunter is an AI-powered recruitment platform: it scrapes job postings, enriches them with an LLM, analyses candidate CVs against open positions, and tracks the hiring pipeline. The solution is organised as a Clean Architecture / CQRS .NET 10 API plus an Angular 21 SPA, deployed as Docker images to a single host via GitHub Actions.

**The skeleton is good.** Layer separation is real and enforced by NetArchTest, use cases are consistently sliced by feature, API versioning, rate limiting, health checks, central package management and feature flags are all in place. This is well above average for a project of this size (193 commits, ~11 months of history).

**The load-bearing internals are not.** The persistence layer bypasses the ASP.NET Core DI and `DbContext` lifecycle entirely, there are no transactions around multi-write use cases, password hashing is a single unsalted SHA-512 pass, and observability is switched off at the entry point. The front-end is on Angular 21 but written in Angular 14 idioms, with effectively zero test coverage.

There is also **one defect that stops the API from booting at all** (§3.1).

### Health scorecard

| Dimension | Rating | One-line verdict |
|---|---|---|
| Layering & structure | 🟢 Good | Clean Architecture honoured, enforced by tests |
| Use-case design (CQRS) | 🟡 Fair | Consistent slicing, but fat handlers and no pipeline behaviours |
| Domain model | 🔴 Poor | Fully anemic; all logic in handlers; domain references ASP.NET Core |
| Persistence | 🔴 Poor | DI bypassed, no transactions, leaked contexts, tracking bugs |
| Security | 🔴 Poor | Weak password hashing, committed secrets, wildcard CORS, non-expiring refresh tokens |
| AI integration | 🟡 Fair | Works, but no resilience, no caching, title-string coupling, injection-prone prompts |
| Observability | 🔴 Poor | Structured logging commented out; no tracing/metrics |
| Backend testing | 🟢 Good | 42 test files, architecture tests, integration tests on real Postgres |
| Front-end testing | 🔴 Poor | 1 spec file for 23 components; never run in CI |
| Front-end architecture | 🟡 Fair | Standalone components, but no signals, no OnPush, legacy DI/interceptors |
| CI/CD | 🟡 Fair | Pipeline exists end-to-end, but no gates, no rollback, password-based SSH |
| Configuration & secrets | 🔴 Poor | Secrets in git, broken config file, env-var sprawl |

---

## 2. System Inventory

### 2.1 Solution topology

```
Hunter.sln
├── src/
│   ├── GeminiCode.Hunter.Api                    24 files   1 428 LOC   (ASP.NET Core 10 Web API)
│   ├── GeminiCode.Hunter.Application.Core      178 files   7 904 LOC   (use cases, services, background jobs)
│   ├── GeminiCode.Hunter.Domain.Core            77 files   1 242 LOC   (entities, interfaces, exceptions)
│   ├── GeminiCode.Hunter.Infra.Data             66 files  13 844 LOC   (EF Core, repos, UoW, migrations)
│   ├── GeminiCode.Hunter.Crosscutting.Ioc         1 file      49 LOC   (DI registration)
│   └── GeminiCode.Hunter.Crosscutting.Resources   3 files     49 LOC   (i18n resources en-US / pt-BR)
├── tests/
│   └── GeminiCode.Hunter.Test                   42 files   5 427 LOC
└── front-end/                                   93 files   6 463 LOC   (Angular 21 SSR-capable SPA)
```

> ⚠️ Of the 13 844 LOC in `Infra.Data`, roughly **11 000 are generated EF migration + Designer files** (23 migrations). Hand-written infrastructure is closer to 2 800 LOC.

### 2.2 Dependency graph (actual)

```
Api ──────────► Application.Core ──► Domain.Core ──► Crosscutting.Resources
 │                                       ▲
 ├──────────► Crosscutting.Ioc ──────────┤
 │                   └──► Infra.Data ────┘
 └──────────► Crosscutting.Resources
```

Direction is correct — nothing points inward-out. `Crosscutting.Ioc` is the only composition point that knows about `Infra.Data`, which is the right call.

### 2.3 Runtime components

| Component | Technology | Notes |
|---|---|---|
| API | ASP.NET Core 10, MVC controllers, MediatR 14 | 9 controllers, 55 handlers, v1 only |
| Database | PostgreSQL 16.3, EF Core 10 | Migrations applied automatically at startup |
| AI | OpenAI SDK 2.10 (OpenAI + DeepSeek via base-URL override) | Provider selected from a DB parameter at call time |
| Background work | 2 × `BackgroundService` in-process | Job sync (hourly, business hours), Looker Studio token refresh |
| Email | MailKit, embedded HTML templates | |
| Scraping | HtmlAgilityPack | `taller.applytojob.com` |
| PDF | PdfPig | CV text extraction |
| Front-end | Angular 21, Material, Quill, SSR via `@angular/ssr` | 23 components, nginx-served |
| Monitoring | New Relic agent | APM only; logging pipeline disabled |

---

## 3. Findings

Severity: 🔴 **Critical** (breaks production or exploitable) · 🟠 **High** (correctness/security risk) · 🟡 **Medium** (maintainability/scale) · 🔵 **Low** (hygiene)

### 3.1 🔴 `appsettings.json` is not valid JSON — the API cannot start

`src/GeminiCode.Hunter.Api/appsettings.json:56` is missing a comma:

```jsonc
"OpenAI": {
  "ApiKey": ""       // ← no comma
  "Model": "gpt-4o"
},
```

Verified with a JSON parser: `Expecting ',' delimiter: line 57 column 5`. `ConfigurationBuilder.AddJsonFile` throws `InvalidDataException` on malformed JSON, so **the host fails before `builder.Build()` returns** in every environment. Introduced by the most recent commit, `49b28da "Remove secrets"`.

This is a one-character fix, but it means HEAD is currently un-runnable and un-deployable.

### 3.2 🔴 Password hashing is a single unsalted SHA-512 pass

`src/GeminiCode.Hunter.Domain.Core/Extensions/StrignExtensions.cs:8`

```csharp
public static string ToSha512(this string text) => /* SHA512.HashData, hex-encoded */;
```

Used for login (`LoginWithCredentialsRequestHandler.cs:19`), user creation, and password change. There is **no salt, no work factor, no per-user uniqueness**. Modern GPUs compute billions of SHA-512 hashes per second; a leaked `User` table is effectively plaintext for any password in a wordlist, and identical passwords produce identical hashes across accounts.

The README advertises this as *"Secure password hashing (SHA-512)"* — the claim should be withdrawn along with the implementation. Target: ASP.NET Core `PasswordHasher<T>` (PBKDF2, versioned) or Argon2id, with transparent rehash-on-login migration.

### 3.3 🔴 Secrets committed to the repository

| Secret | Location |
|---|---|
| New Relic license key `88c9223c…FFFFNRAL` | `Dockerfile:16` — baked into every published image |
| JWT signing key | `appsettings.Development.json`, `appsettings.Test.json` |
| Postgres password `123PgSql` | dev/test appsettings, `.github/workflows/build.yml` |
| Seeded admin password `"123"` | `src/GeminiCode.Hunter.Infra.Data/Mapping/UserMapping.cs:42` |

The last one is the worst: `UserMapping` seeds `adm@hunter.com` with the SHA-512 of `"123"` via `HasData`, so **every environment that runs migrations — including production — gets a known-credential admin account**. Migrations run automatically on boot (`MigrationsConfiguration.cs`), so this is not hypothetical.

Note that commit `49b28da "Remove secrets"` cleaned some values but left these, and broke the config file in the process (§3.1). Any key that was ever committed must be treated as burned and rotated at the provider — removing it from the working tree does not remove it from git history.

### 3.4 🔴 The persistence layer bypasses DI and leaks `DbContext` instances

`src/GeminiCode.Hunter.Infra.Data/UnitOfWork/UnitOfWork.cs:17`

```csharp
public sealed class UnitOfWork(IConfiguration configuration, ILogger<UserRepository> logger) : IUnitOfWork
{
    private DataContext DbContext { get; } = new(configuration);
```

Compounded by `DataContext` constructing its own options (`DataContext.cs:12`):

```csharp
public class DataContext(IConfiguration configuration) : DbContext(GetDbContextOptions(configuration))
```

Consequences, all of them real:

1. **`AddDbContext` is never called.** `DependenciesInjectionConfiguration.AddDatabaseContext` registers `AddScoped<DataContext>()` only — so there is no pooling, no `EnableRetryOnFailure`, no `CommandTimeout`, no integration with the logging/diagnostics pipeline, no design-time factory alignment.
2. **`IUnitOfWork` is registered `Transient`.** Every injection site constructs a *new* `DataContext` with a *new* connection. A single HTTP request that touches a handler plus a service gets multiple independent contexts that cannot see each other's tracked changes.
3. **The context is never disposed.** `UnitOfWork.Dispose(bool)` sets a `_disposed` flag and does nothing else — `DbContext.Dispose()` is never called. Under load this leaks pooled connections until the Npgsql pool is exhausted.
4. **Connection string is read per-instantiation** from `IConfiguration`, defeating `IOptions` validation and hot-reload semantics.
5. The scoped `DataContext` registration is simultaneously *unused by the UoW* and *used directly* by `MigrationsConfiguration` — two different context lifetimes coexist.

Also `ILogger<UserRepository>` is injected into `UnitOfWork` — a copy-paste artefact that makes every UoW log line appear under the wrong category.

### 3.5 🔴 No transactions; multi-write use cases can persist partial state

`grep` for `IPipelineBehavior|BeginTransaction|IDbContextTransaction` across `src/` returns **zero matches**.

`AnalyzeCvRequestHandler` is the worst case — it calls `CommitAsync()` **four separate times** across three private methods (candidate upsert → PDF blob → analysis header → analysis matches). Worse, two of those methods swallow their own failures:

```csharp
catch (Exception ex)
{
    logger.LogError(ex, "Error storing CV PDF for candidate {CandidateId}", candidateId);
}   // ← swallowed; the request returns 200 OK
```

So a failure mid-way through leaves a candidate row with no résumé, or an analysis header with no matches, and the caller is told everything succeeded. There is no compensating action and no way for the user to tell.

### 3.6 🟠 `ChangeTracker.Clear()` inside the generic delete discards unrelated pending work

`src/GeminiCode.Hunter.Infra.Data/Repository/Generic/BaseEntityRepository.cs:31`

```csharp
public virtual async Task DeleteAsync(string publicId)
{
    var existing = await GetByIdAsync(publicId);
    if (existing != null)
    {
        DataContext.ChangeTracker.Clear();   // ← detaches EVERYTHING
        DataContext.Set<T>().Remove(existing);
    }
}
```

`Clear()` detaches every tracked entity in the context, so any `AddAsync`/`Update` queued earlier in the same unit of work is **silently dropped** before `CommitAsync` runs. This is a workaround for a tracking conflict caused by `GetByIdAsync` using `AsNoTracking()` — the fix is to stop fighting the change tracker, not to flush it.

The same `AsNoTracking()` + `Entry(entity).State = Modified` + `Update(entity)` combination in `Update` forces full-row UPDATE statements on every column and prevents optimistic concurrency (there is no `xmin`/rowversion token on `BaseEntity`).

### 3.7 🟠 Refresh tokens never expire, never rotate, and are shared per email

`RefreshTokenMapping.cs` defines no expiry column. `LoginWithCredentialsRequestHandler.cs:50-52`:

```csharp
var existentRefreshToken = (await unitOfWork.RefreshTokenRepository.SearchByAsync(x => x.Email == userEmail)).FirstOrDefault();
if (existentRefreshToken is not null)
    return existentRefreshToken;   // same token forever
```

With `HasIndex(x => x.Email).IsUnique()` there is exactly one refresh token per user, issued once and **valid indefinitely**. No rotation, no revocation list, no device binding, no expiry, no invalidation on password change or logout. A refresh token captured once grants permanent access; the 120-minute JWT lifetime provides no real containment.

### 3.8 🟠 CORS allows any origin, and Swagger is public in production

`src/GeminiCode.Hunter.Api/Bootstrapper.cs:28`

```csharp
app.UseCors(options => options.WithOrigins("*").AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader());
```

`WithOrigins("*")` and `AllowAnyOrigin()` are redundant *and* contradictory in intent. For a Bearer-token API this is less catastrophic than with cookies, but it removes any origin-based defence and will block the addition of cookie auth later.

`app.UseSwagger()` / `UseSwaggerUI()` at lines 21–25 are unconditional — the full API surface, including admin and prompt-management endpoints, is documented publicly in production. Swagger should be environment-gated.

### 3.9 🟠 Exception middleware leaks internal exception messages

`src/GeminiCode.Hunter.Api/Middleware/ExceptionMiddleware.cs:28`

```csharp
var errorResponse = new ExceptionResponse(exception.Message, NotificationType.Error);
```

For unhandled exceptions the `Title` is replaced with `"Unexpected error"`, but `Errors[0]` still carries the raw `exception.Message` — which for Npgsql, EF, or the OpenAI SDK means table names, SQL fragments, connection details, or upstream API errors are returned to the client with a 500. Only `BusinessException` subclasses should surface their message.

Secondary: this is the only place using Newtonsoft.Json; the rest of the codebase uses `System.Text.Json`. Two serializers for one app is avoidable weight.

### 3.10 🟠 AES encryption uses a static, key-derived IV with no authentication

`src/GeminiCode.Hunter.Application.Core/Services/EncryptionService.cs:29-30`

```csharp
_key = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
_iv  = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey + "_iv"))[..16];
```

A fixed IV across every ciphertext makes AES-CBC deterministic: identical plaintexts produce identical ciphertexts, leaking equality and enabling chosen-ciphertext analysis. There is no MAC, so ciphertexts are malleable and decryption failures are distinguishable — the classic padding-oracle shape. This protects Looker Studio credentials, which are real third-party secrets.

Target: `AesGcm` with a random 96-bit nonce per message, or ASP.NET Core Data Protection.

### 3.11 🟠 Observability is disabled at the entry point

`src/GeminiCode.Hunter.Api/Program.cs:26-28`

```csharp
//TODO: Temp disabled due server resource limits
//builder.Configuration.ConfigureLogging();
//builder.Host.UseSerilog();
```

Serilog, correlation-ID enrichment and environment enrichment are all installed as packages and then switched off. The result is default console logging with no correlation IDs, no structured sinks, and no way to trace a CV-analysis request across the 5-minute timeout window. `docker-compose.prod.yml` still passes `ElasticConfiguration__Uri` and declares an `elasticsearch-data` volume for a stack that no longer exists.

Combined with the swallowed exceptions in §3.5, failures in production are currently **invisible**. There is also no OpenTelemetry, no metrics, and no tracing — New Relic's auto-instrumentation is the only signal.

### 3.12 🟠 AI integration is coupled to job *titles* as identity

`AnalyzeCvRequestHandler.cs:97` and `:246`

```csharp
var job = jobPositions.First(j => j.Title == match.JobTitle);
```

The LLM is asked to echo job titles back, and the handler then does an exact-string lookup. `First` throws `InvalidOperationException` if the model paraphrases, trims, re-cases, or hallucinates a title — and silently picks the wrong row when two positions share a title. Nothing in the prompt or schema guarantees verbatim echo.

Relatedly, `AIService.GetResponseSchema()` (`AIService.cs:318, 332`) still **requires** a `jobCode` field in the strict JSON schema, even though `JobCode` was removed from the domain in commits `63556ae`/`c9bf529`. The model is being forced to invent a value that is then discarded — a stale contract that will confuse the next person to touch it.

The fix is to pass a stable opaque identifier (`PublicId`) into the prompt and match on that.

### 3.13 🟡 `AIService` mutates shared state per call and has no resilience

`AIService` is registered `Scoped` but holds `_chatClient`, `_provider` and `_model` as mutable instance fields, reassigned by `SetupProvider()` **on every public method call** — a fresh `ChatClient` (and underlying HTTP stack) per CV analysis. It does not use `IHttpClientFactory`.

Missing entirely: retry with backoff, circuit breaker, per-call timeout, token budgeting, cost tracking, and any caching of the parameter/prompt lookups that hit the database on every single AI call.

Prompts are assembled by `string.Replace` of `{VAR_x}` placeholders with **scraped HTML** and **candidate CV text** (`BuildJobEnrichmentPromptAsync`, `BuildAnalysisPromptAsync`). Both are attacker-influenceable: a candidate can embed instructions in their CV, and a job board can embed them in a posting. There is no delimiting, no instruction-hierarchy defence, and no output validation beyond schema shape.

### 3.14 🟡 The domain model is anemic and depends on ASP.NET Core

Every entity is a property bag. `JobPosition` has `IsActive`, `ClosedAt` and `LastCheckedAt` with no `Close()` method; `HiringProcess` has a `Status` with no transition rules — the Referred → Screening → … → Hired state machine lives in handlers and is re-implemented per use case. `BaseEntity` exposes `PublicId`, `CreatedAt` and `LastUpdated` as public setters, and `PublicId` is assigned by the `DbContext` (`DataContext.AddTimestamps`), so an entity is not valid until it reaches the database.

Structurally, `GeminiCode.Hunter.Domain.Core.csproj` declares `<FrameworkReference Include="Microsoft.AspNetCore.App" />` and the domain hosts `ICurrentUser`/`CurrentUser` built on `IHttpContextAccessor`. Request DTOs carry `IFormFile`. **The domain layer depends on the web framework** — the architecture tests do not catch this because they only assert on project namespaces, not on framework references.

There are no value objects (`Email`, `MatchPercentage`, `PhoneNumber` are all raw primitives) and no domain events — MediatR `INotification`s are published from handlers, which is application-level orchestration, not domain behaviour.

### 3.15 🟡 No MediatR pipeline behaviours

Validation runs in an MVC action filter (`AutoValidationActionFilter`), which means it only protects the HTTP entry point. Requests dispatched from `JobSyncBackgroundService` through `IMediator` are **never validated**. Logging, transaction scoping, performance timing and authorization are likewise absent from the pipeline and scattered (or missing) per handler.

This is the single highest-leverage structural gap: a `ValidationBehavior`, `TransactionBehavior`, `LoggingBehavior` and `PerformanceBehavior` would fix §3.5 and most of the per-handler duplication at once.

### 3.16 🟡 `CancellationToken` is accepted and then discarded

All 55 handlers receive a `CancellationToken`. **None of them pass it downstream** — `IUnitOfWork.CommitAsync()` takes no token, `DataContext.SaveChangesAsync()` is the parameterless overload, and no repository method accepts one. Combined with a 5-minute request timeout on CV analysis, an abandoned client request keeps burning a database connection and an LLM call to completion.

### 3.17 🟡 Files are stored as `bytea` rows in PostgreSQL

`FileMapping.cs` maps `FileData` to `bytea`; `StorageType` and the `S3Key`/`S3Bucket` columns exist but only `StorageType.Database` is ever written. Every résumé is loaded fully into memory on upload (`MemoryStream` → `ToArray()`) and again on download. With a 10 MB Kestrel body limit this bloats the database, inflates every backup, and makes `SELECT *`-style queries on `File` dangerous. The S3 columns suggest this was recognised and deferred.

### 3.18 🟡 Background services cannot be scaled horizontally

Both hosted services run in-process as singletons with no distributed lock or leader election. A second API replica means **duplicate job scraping, duplicate AI enrichment spend, and duplicate notification emails**. `JobSyncBackgroundService` also mixes `DateTime.Now` (server-local) with `TimeOnly` windows commented as UTC — correct only if the host happens to be UTC, which contradicts the `America/Sao_Paulo` setting elsewhere. `CalculateNextRunTime` hand-rolls business-day arithmetic that a cron expression would express in one line.

### 3.19 🟡 `DataContext.OnModelCreating` overrides all table naming after the fact

`DataContext.cs:58`

```csharp
foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    modelBuilder.Entity(entityType.ClrType).ToTable(entityType.ClrType.Name);
```

This runs after the 14 `IEntityTypeConfiguration` registrations and silently discards any `ToTable` they set. The 14 `ApplyConfiguration` calls are also manual — `ApplyConfigurationsFromAssembly` would keep this list from drifting.

### 3.20 🟡 Front-end: Angular 21 running Angular 14 patterns

23 components, 6 463 LOC. Standalone components are used (9 files), which is good. Everything else is legacy:

| Pattern | Current | Modern equivalent |
|---|---|---|
| State | `BehaviorSubject` + `async` pipe | `signal()` / `computed()` — **0 usages found** |
| Change detection | Default (zone-based) | `ChangeDetectionStrategy.OnPush` — **0 usages found** |
| DI | Constructor parameters | `inject()` |
| Interceptors | Class + `HTTP_INTERCEPTORS` + `withInterceptorsFromDi()` | Functional `withInterceptors([...])` |
| Zone | `zone.js` present | `provideZonelessChangeDetection()` |
| Inputs/outputs | Decorators | `input()` / `output()` signals |

Token storage is `localStorage` (`auth.service.ts:12-13`), which is XSS-exfiltratable; the refresh-token design in §3.7 makes a single successful XSS permanent.

Both the CI workflow and `front-end/Dockerfile` use `npm install --legacy-peer-deps` — an unresolved peer-dependency conflict is being papered over, and `npm install` (rather than `npm ci`) means builds are not reproducible from the lockfile.

### 3.21 🟡 Front-end has no meaningful tests, and CI never runs them

One spec file (`app.component.spec.ts`) for 23 components and 12 services. `npm test` is defined in `package.json` but **absent from `.github/workflows/build.yml`** — the pipeline builds the front-end and never tests it. Karma/Jasmine is also the legacy harness; new Angular projects default to Vitest.

### 3.22 🟡 Backend tests are solid but partly disabled and ungated

The backend test suite is genuinely good: 7 architecture-test files (NetArchTest), integration tests against a real Postgres with per-run database creation, Bogus fakers, controller and handler unit tests.

Two problems:

- `tests/GeminiCode.Hunter.Test/GeminiCode.Hunter.Test.csproj:9` — `<Compile Remove="Unit\Application\JobPositions\**" />` **excludes an entire test folder from compilation**. Those tests exist on disk and never run. Either fix them or delete them; leaving them is worse than both.
- Coverage is collected (`coverlet`, OpenCover format, copied to `opencover.xml`) and then **never published or enforced**. The JDK 17 setup step in the workflow is a leftover from a removed SonarQube integration.

### 3.23 🟡 CI/CD has no gates, no staging, and no rollback

`deploy.yml` triggers on every push to `master`, builds both images, pushes `:latest`, SSHes to a single production host and runs `deploy.sh`. Issues:

- **SSH uses password authentication** (`secrets.HUNTER_PROD_PASSWORD`) rather than a key.
- **`deploy.sh:20` has a typo:** `docker compose up fronted api --build -d` — `fronted` is not a service in `docker-compose.prod.yml`. With `set -e` at the top, this line fails the script, so the deploy either aborts or never actually starts the frontend.
- `--build` on the production host contradicts the pull-prebuilt-images model the rest of the script uses.
- `sleep 3` then health-check; no readiness gate, no smoke tests, no automatic rollback, no blue/green. A bad `:latest` is live until someone notices.
- `build.yml` and `deploy.yml` are independent — **deploy does not depend on build passing**.
- No security scanning: no CodeQL, no `npm audit`, no Trivy image scan, no secret scanning. Dependabot is configured but is the only automated check.

### 3.24 🔵 Container and compose hygiene

- `Dockerfile` does `COPY . ./` *before* `dotnet restore`, so **every source change invalidates the restore layer**. Copying `.csproj`/`Directory.Packages.props` first would cache restores.
- The runtime image runs as **root**, has no `HEALTHCHECK`, and is not trimmed or AOT-compiled.
- `docker-compose.prod.yml` publishes **PostgreSQL on host port 5432** — the database is reachable from outside the Docker network. It should be internal-only.
- No resource limits (`mem_limit`/`cpus`) on any service, on a host the code comments describe as resource-constrained (§3.11).
- Orphaned `elasticsearch-data` volume.

### 3.25 🔵 Naming, typos and inconsistencies

Low individual impact, high cumulative friction:

- `Entities/Enuns/` — pt-BR spelling of "Enums", in a public namespace
- `StrignExtensions.cs` — transposed "String"
- `NotificationType.Warnning`, `ProccessResponseAsync`, `errrorReturn`
- Repositories split across `Repository/` (6 files) and `Repository/Implementations/` (6 files) with no discernible rule
- `PagedList<T>` carries only `Items` + `Count` — no page number, page size, or total pages, so clients cannot render a pager without extra calls
- Mixed `_camelCase` fields vs. primary-constructor parameters across services (both styles appear in the same project)
- MediatR 14.1.1 — v13+ is **commercially licensed**; worth confirming the licence position before investing further in it

---

## 4. What Is Working Well

Worth protecting during modernization:

- **Clean Architecture is real, not aspirational.** Dependencies point inward and 7 NetArchTest files enforce it on every build.
- **Vertical slicing by use case** — `UseCases/{Feature}/{Commands|Queries|Notifications}/{Operation}/` with request/handler/response/validator colocated. Consistent across all 55 handlers.
- **Central package management** via `Directory.Packages.props` — no version drift between projects.
- **Integration tests against real PostgreSQL** with isolated per-run databases — far better than in-memory-provider testing.
- **API versioning, rate limiting (per-endpoint), health checks, feature flags, i18n (en-US/pt-BR)** all present and wired.
- **Modern C#** — primary constructors, collection expressions, records, file-scoped namespaces, nullable reference types in newer projects.
- **Dual AI provider support** with runtime switching via a database parameter — a genuinely good cost-control design.
- **Full CI/CD path** from push to deployed, including health checks. The shape is right even where the rigour is missing.

---

## 5. Prioritised Remediation Roadmap

### Phase 0 — Stop the bleeding (hours)

| # | Action | Finding |
|---|---|---|
| 1 | Fix the missing comma in `appsettings.json` | §3.1 |
| 2 | Rotate the New Relic key, JWT keys and DB passwords at the provider; move to env/secret store | §3.3 |
| 3 | Remove the seeded `adm@hunter.com` / `"123"` account; replace with first-run bootstrap | §3.3 |
| 4 | Fix `deploy.sh` `fronted` → `frontend`; drop `--build` | §3.23 |
| 5 | Gate Swagger behind non-production; stop leaking `exception.Message` on 500s | §3.8, §3.9 |

### Phase 1 — Security (1–2 weeks)

| # | Action | Finding |
|---|---|---|
| 6 | Replace SHA-512 with `PasswordHasher<User>`/Argon2id + rehash-on-login migration | §3.2 |
| 7 | Add refresh-token expiry, rotation-on-use, and revocation; invalidate on password change | §3.7 |
| 8 | Replace AES-CBC/static-IV with AES-GCM or Data Protection; re-encrypt stored credentials | §3.10 |
| 9 | Restrict CORS to the known front-end origins | §3.8 |
| 10 | Move front-end tokens out of `localStorage` (in-memory + httpOnly refresh cookie) | §3.20 |
| 11 | Add CodeQL, `npm audit`, Trivy and secret scanning to CI | §3.23 |

### Phase 2 — Persistence correctness (2–3 weeks)

| # | Action | Finding |
|---|---|---|
| 12 | Register `DataContext` via `AddDbContext` with Npgsql retry + timeouts; delete `GetDbContextOptions` | §3.4 |
| 13 | Make `UnitOfWork` scoped and inject `DataContext`; implement real `Dispose`/`IAsyncDisposable` | §3.4 |
| 14 | Add a MediatR `TransactionBehavior`; reduce `AnalyzeCvRequestHandler` to a single commit | §3.5 |
| 15 | Remove `ChangeTracker.Clear()`; stop mixing `AsNoTracking` with `Update` | §3.6 |
| 16 | Thread `CancellationToken` through repositories and `CommitAsync` | §3.16 |
| 17 | Add a concurrency token (`xmin`) to `BaseEntity` | §3.6 |
| 18 | Replace the `ToTable` loop with `ApplyConfigurationsFromAssembly` | §3.19 |
| 19 | Squash 23 migrations into a baseline; stop auto-migrating on boot in production | §2.1, §3.3 |

### Phase 3 — Architecture & AI (3–4 weeks)

| # | Action | Finding |
|---|---|---|
| 20 | Add `Validation`, `Logging`, `Performance` pipeline behaviours | §3.15 |
| 21 | Remove `FrameworkReference`/`IHttpContextAccessor` from `Domain.Core`; move `ICurrentUser` to Application, implementation to API | §3.14 |
| 22 | Introduce value objects and move the `HiringProcessStatus` transition rules into the entity | §3.14 |
| 23 | Extend architecture tests to assert framework references, not just namespaces | §3.14 |
| 24 | Match AI results on `PublicId`, not `Title`; drop the stale `jobCode` from the schema | §3.12 |
| 25 | Give `AIService` `IHttpClientFactory` + Polly resilience + per-call timeout + cost logging | §3.13 |
| 26 | Delimit and defend untrusted prompt inputs (scraped HTML, CV text); validate model output | §3.13 |
| 27 | Cache `ParameterService`/`PromptService` lookups | §3.13 |
| 28 | Restore Serilog; add OpenTelemetry traces + metrics | §3.11 |
| 29 | Move file storage to S3/MinIO behind the existing `StorageType` abstraction; stream instead of buffering | §3.17 |
| 30 | Move background jobs to Quartz/Hangfire with a distributed lock; fix the `DateTime.Now`/UTC mismatch | §3.18 |

### Phase 4 — Front-end & delivery (3–4 weeks)

| # | Action | Finding |
|---|---|---|
| 31 | Migrate to signals + `OnPush`, then evaluate zoneless | §3.20 |
| 32 | Convert to functional interceptors and `inject()` | §3.20 |
| 33 | Resolve the peer-dependency conflict; switch to `npm ci` | §3.20 |
| 34 | Stand up front-end tests (Vitest) and run them in CI | §3.21 |
| 35 | Re-enable or delete the excluded `JobPositions` tests; publish and gate coverage | §3.22 |
| 36 | Make `deploy` depend on `build`; add staging, smoke tests and rollback; switch SSH to key auth | §3.23 |
| 37 | Dockerfile: layer-cached restore, non-root user, `HEALTHCHECK`; unpublish Postgres; add resource limits | §3.24 |
| 38 | Rename `Enuns`, `StrignExtensions`, `Warnning`, `Proccess…`; consolidate the repository folders; enrich `PagedList<T>` | §3.25 |

---

## 6. Suggested Sequencing

Phases 0 and 1 are non-negotiable prerequisites — the system does not currently start, and when it does it ships a known-credential admin account.

Phase 2 is the highest-value structural work: items 12–14 alone eliminate the connection leak, the partial-write failure mode, and the silent-data-loss path, and they unblock most of Phase 3.

Phases 3 and 4 are independent of each other and can run in parallel if there is capacity.

**Recommended first slice:** Phase 0 in full, then items 6, 12, 13 and 14 as a single coherent change — that combination takes the project from "cannot boot, leaks connections, loses data silently" to "boots, scales to one replica safely, fails loudly."

---

*Analysis based on static inspection of the working tree at `49b28da`. No code was modified. Line references are accurate as of that commit.*
