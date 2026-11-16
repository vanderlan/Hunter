# Public job board and self-service CV match

**Added:** 2026-11-15

Hunter used to be fully closed: every screen and every API endpoint required a login.
This feature opens a public, anonymous surface so people outside the platform can see
the open positions, match their own CV and ask someone inside to refer them.

## What a visitor can do (no account)

| Step | Where | What happens |
|---|---|---|
| Browse positions | `/` and `/jobs/:code` | Lists active positions that already have company-neutral public content. |
| Match a CV | `/match` | Uploads a PDF, the AI compares it with the public content of every open position and returns match %, matching and missing skills. The CV is stored under a *public candidate* that is invisible to internal users. |
| Save profile | dialog on `/match` | Name, email, phone, notification opt-in and terms acceptance. A confirmation link is emailed. |
| Confirm email | `/confirm?token=…` | Marks the profile as confirmed; it becomes visible internally and any held referral requests are delivered. |
| Ask for a referral | button on each match | Creates a *referral request*. Internal users are emailed (immediately if the email is confirmed, otherwise on confirmation). |

Nothing returned by the public API contains internal ids, the external job URL, priority,
country restrictions or the company name. Positions are addressed by an opaque `PublicCode`,
the visitor by an opaque `PublicToken`.

## What internal users see

* **Referral Requests** (new screen, Admin and Partner): pending requests with candidate,
  position, AI match and the candidate's message. **Refer** starts a regular hiring process
  in the name of the user who clicked, exactly like referring from the CV analyzer, and
  notifies admins. **Decline** keeps the request with a note.
* **Candidates**: public candidates appear with a `Public` badge only after confirming
  their email. Partners don't see them in their own list (no owner user); they reach them
  through Referral Requests.
* The internal CV analyzer moved from `/` to `/analyze`. The root is now the public job board.

## Company-neutral content

On every sync, new positions get an extra AI call that rewrites summary, responsibilities,
requirements and skills removing anything that identifies the company. Active positions
that still lack public content are back-filled on the next sync. A position is only
listed publicly once that content exists.

The prompt has a built-in default. To customise it, create a prompt with key
`PUBLIC_JOB_CONTENT` in **AI Settings**; the placeholders are `{VAR_jobTitle}`,
`{VAR_summary}`, `{VAR_responsibilities}`, `{VAR_requirements}`, `{VAR_mustHaveSkills}`,
`{VAR_niceToHaveSkills}`.

## Abuse and cost protection

| Control | Where | Default |
|---|---|---|
| Per-IP limit on the public CV match | `RateLimiting:PublicCvMatch` in `appsettings.json` | 3 per hour |
| Per-IP limit on other public endpoints | `RateLimiting:Public` | 120 per minute |
| Global daily cap on public CV analyses | parameter `PUBLIC_CV_MATCH_DAILY_LIMIT` | 200 |
| Minimum match to ask for a referral | parameter `MINIMUM_MATCH_PERCENTAGE_REFERRAL_REQUEST` | 50 |

Per-IP limits rely on `X-Forwarded-For`, so the API now runs `UseForwardedHeaders` and
trusts the proxy in front of it (nginx in the compose stack).

## Data retention

`PublicCandidateCleanupBackgroundService` runs every 6 hours and deletes:

* public CVs whose owner never saved a profile, after 24 hours;
* saved profiles whose email was never confirmed, after 7 days.

Confirmation links expire after 7 days. Registering again re-sends the link.

## Schema changes (migration `AddPublicJobBoardAndReferralRequests`)

* `JobPosition`: `PublicCode` (unique), `PublicSummary`, `PublicResponsibilities`,
  `PublicRequirements`, `PublicMustHaveSkills`, `PublicNiceToHaveSkills`,
  `PublicContentGeneratedAt`. Existing rows receive a generated code in the migration.
* `Candidate`: `UserId` is now nullable; new `Source`, `PublicToken`, `HasAcceptedTerms`,
  `TermsAcceptedAt`, `IsEmailConfirmed`, `EmailConfirmationToken`,
  `EmailConfirmationSentAt`, `EmailConfirmedAt`, `ReceiveJobMatchNotifications`.
* `CvAnalysis`: `UserId` is now nullable.
* New table `ReferralRequest`.

## Not in this release

* Email notifications to saved candidates when a new position matches them. The opt-in
  (`ReceiveJobMatchNotifications`) and the confirmed email are already stored, so this is
  a handler on `JobChangesDetectedNotification` away.
* Server-side rendering of the public pages for SEO. The app ships with Angular SSR but
  production serves the static build through nginx.
* CAPTCHA on the CV upload. Add it if the per-IP limits prove insufficient.
