# DeepSeek Integration - Complete ?

## ?? Implementation Complete!

The Hunter now supports **both OpenAI and DeepSeek** with easy switching via configuration. OpenAI is the default provider for stability, but you can switch to DeepSeek to save **90% on AI costs**.

---

## ?? Cost Savings

| Provider | Model | Cost per 1M tokens (Input/Output) | Monthly Cost (Typical Usage) |
|----------|-------|-----------------------------------|------------------------------|
| **OpenAI (Default)** | GPT-4o | $2.50 / $10.00 | ~$15.60/month |
| **DeepSeek** | deepseek-chat | $0.27 / $1.10 | ~$1.56/month |

**Potential Annual Savings: ~$168/year** ??

---

## ?? Configuration

### Current Setup (OpenAI - Default)

Your `appsettings.json` already has OpenAI configured:

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-openai-key",
    "Model": "gpt-4o"
  }
}
```

**This continues to work as before. No changes needed!**

---

## ?? How to Switch to DeepSeek

### Step 1: Get DeepSeek API Key

1. Visit: https://platform.deepseek.com
2. Sign up for a free account
3. Navigate to **API Keys** section
4. Create new API key
5. Copy the key (starts with `sk-`)

### Step 2: Add DeepSeek Configuration

Update your `appsettings.json`:

```json
{
  "AI": {
    "Provider": "DeepSeek"  // Add this to switch provider
  },
  "OpenAI": {
    "ApiKey": "sk-your-openai-key",  // Keep as fallback
    "Model": "gpt-4o"
  },
  "DeepSeek": {
    "ApiKey": "sk-your-deepseek-key",  // Add your DeepSeek key
    "Model": "deepseek-chat"
  }
}
```

### Step 3: Restart Application

```bash
cd src/GeminiCode.Hunter.Api
dotnet run
```

**Check the logs:**
```
[INF] Using DeepSeek AI provider with model: deepseek-chat
```

---

## ?? Switching Between Providers

### Use OpenAI (Default - No config needed)
```json
{
  // Don't add AI:Provider, or set it to "OpenAI"
  "AI": {
    "Provider": "OpenAI"  // Optional, this is the default
  }
}
```

### Use DeepSeek (90% Cost Savings)
```json
{
  "AI": {
    "Provider": "DeepSeek"
  }
}
```

**That's it! Just change one setting and restart.**

---

## ?? Environment Variables (Production)

### For OpenAI (Default)
```bash
# No AI__Provider needed, OpenAI is default
OpenAI__ApiKey=sk-your-openai-key
OpenAI__Model=gpt-4o
```

### For DeepSeek
```bash
AI__Provider=DeepSeek
DeepSeek__ApiKey=sk-your-deepseek-key
DeepSeek__Model=deepseek-chat

# Keep OpenAI as fallback
OpenAI__ApiKey=sk-your-openai-key
OpenAI__Model=gpt-4o
```

### Docker Compose

Update `docker-compose.yml`:

```yaml
services:
  api:
    environment:
      - AI__Provider=DeepSeek  # Add this to switch
      - DeepSeek__ApiKey=${DEEPSEEK_API_KEY}
      - OpenAI__ApiKey=${OPENAI_API_KEY}
```

Then in `.env` file:
```bash
DEEPSEEK_API_KEY=sk-your-deepseek-key
OPENAI_API_KEY=sk-your-openai-key
```

---

## ?? What Changed

### Backend Service Updated

**File:** `src/GeminiCode.Hunter.Application.Core/Services/OpenAIService.cs`

**Changes:**
- ? Added support for multiple AI providers
- ? Configurable via `AI:Provider` setting
- ? OpenAI remains the default (no breaking changes)
- ? DeepSeek uses same OpenAI SDK (100% compatible)
- ? Logging shows which provider is being used

### Logging Updates

**Before:**
```
[INF] Starting CV analysis against 5 positions using gpt-4o
```

**After:**
```
[INF] Using OpenAI provider with model: gpt-4o
[INF] Starting CV analysis against 5 positions using OpenAI (gpt-4o)
```

Or with DeepSeek:
```
[INF] Using DeepSeek AI provider with model: deepseek-chat
[INF] Starting CV analysis against 5 positions using DeepSeek (deepseek-chat)
```

---

## ?? Testing DeepSeek

### Test 1: Job Sync
```bash
curl -X POST "http://localhost:5010/JobPositions/Sync" \
  -H "Authorization: Bearer <token>" \
  -H "x-api-version: 1.0"
```

**Check logs for:**
```
[INF] Enriching job details for: Senior Developer using DeepSeek
[INF] Successfully enriched job details for: Senior Developer using DeepSeek
```

### Test 2: CV Analysis
1. Navigate to http://localhost:4200
2. Upload a CV
3. Click "Analyze CV"
4. Check API logs for: `Starting CV analysis against X positions using DeepSeek`

### Test 3: Compare Results

**Week 1:** Use OpenAI
- Run 10 CV analyses
- Note match percentages and quality

**Week 2:** Switch to DeepSeek
- Run same 10 CVs
- Compare results
- Expected: 85-90% similar quality at 10% of the cost

---

## ? Compatibility

DeepSeek supports all features:
- ? **Chat Completions API** (same as OpenAI)
- ? **System and User messages**
- ? **Temperature control**
- ? **JSON Schema mode** (structured output)
- ? **Max tokens control**
- ? **Same SDK** (OpenAI .NET package)

**No code changes needed!** Just configuration.

---

## ?? Expected Results

### Job Enrichment
**Quality:** Excellent on both providers
**Speed:** Similar (2-4 seconds)
**Cost:** DeepSeek 90% cheaper

### CV Analysis
**Quality:** OpenAI slightly better, DeepSeek very good
**Speed:** Similar
**Cost:** DeepSeek 90% cheaper

### Recommendation
Start with job enrichment on DeepSeek (low risk, high savings). If satisfied after 1-2 weeks, switch CV analysis too.

---

## ?? Recommended Migration Strategy

### Phase 1: Testing (Week 1)
```json
{
  "AI": {
    "Provider": "DeepSeek"
  }
}
```
- Test all features
- Compare quality
- Monitor costs
- Check logs for errors

### Phase 2: Evaluation (Week 2)
- Analyze results
- Calculate actual savings
- Decide: keep DeepSeek or revert

### Phase 3: Production (Week 3+)
- Deploy to production if satisfied
- Monitor ongoing
- Keep OpenAI key as emergency fallback

---

## ?? Known Limitations

1. **Context Window**
   - OpenAI: 128K tokens
   - DeepSeek: 64K tokens
   - **Impact:** None (CVs rarely exceed 10K tokens)

2. **Latency**
   - DeepSeek might be 100-200ms slower
   - **Impact:** Minimal (users won't notice)

3. **Regional**
   - DeepSeek servers in China
   - **Impact:** Slight latency from US/EU

**None of these impact the Hunter use case!**

---

## ?? Security Best Practices

### Don't Commit API Keys
```bash
# .gitignore (already in place)
appsettings.Development.json
appsettings.Production.json
.env
```

### Use Azure Key Vault (Production)
```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

### Use User Secrets (Development)
```bash
cd src/GeminiCode.Hunter.Api
dotnet user-secrets set "DeepSeek:ApiKey" "sk-your-key"
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-key"
```

---

## ?? Real Cost Example

### Current OpenAI Usage (estimated):
- **Job Enrichment:** 50 jobs/month = 170K tokens = $0.60/month
- **CV Analysis:** 100 CVs/month = 15M tokens = $15/month
- **Total:** $15.60/month = $187/year

### With DeepSeek:
- **Job Enrichment:** 50 jobs/month = 170K tokens = $0.06/month
- **CV Analysis:** 100 CVs/month = 15M tokens = $1.50/month
- **Total:** $1.56/month = $18.72/year

**Annual Savings: $168** ??

---

## ?? What You Learned

? OpenAI and DeepSeek use the same API format
? Same .NET SDK works for both
? Easy to switch via configuration
? 90% cost savings possible
? No code changes needed
? Can switch back anytime

---

## ?? Resources

- **DeepSeek Platform:** https://platform.deepseek.com
- **DeepSeek API Docs:** https://platform.deepseek.com/docs
- **OpenAI Compatibility:** https://platform.deepseek.com/api-docs/openai-compatible
- **Pricing:** https://platform.deepseek.com/pricing

---

## ? Status

**Implementation:** ? Complete
**Build:** ? Successful
**Breaking Changes:** ? None (OpenAI is default)
**Ready to Use:** ? Yes

---

## ?? Quick Start Commands

### Switch to DeepSeek
```bash
# Edit appsettings.json, add:
{
  "AI": {
    "Provider": "DeepSeek"
  },
  "DeepSeek": {
    "ApiKey": "sk-your-key",
    "Model": "deepseek-chat"
  }
}

# Restart API
cd src/GeminiCode.Hunter.Api
dotnet run
```

### Switch Back to OpenAI
```bash
# Edit appsettings.json, change:
{
  "AI": {
    "Provider": "OpenAI"  # or remove this line
  }
}

# Restart API
dotnet run
```

---

## ?? Summary

**You now have:**
- ? Dual AI provider support (OpenAI + DeepSeek)
- ? Easy switching via configuration
- ? 90% potential cost savings
- ? No code changes required
- ? OpenAI as safe default
- ? Full backward compatibility

**Test DeepSeek risk-free and save money!** ??

---

**Questions?** Check the logs - they show which provider is active:
```bash
docker-compose logs -f api | grep "Using.*provider"
```

**Happy cost-saving!** ??
