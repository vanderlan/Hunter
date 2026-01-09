# Environment Variables Quick Reference

## ?? `.env` File Generated

I've created a `.env` file with **all current values** from your `appsettings.json` files. This file contains sensitive credentials and should **NOT** be committed to Git.

## ?? **IMPORTANT SECURITY NOTES**

### **Exposed Credentials Found:**

Your repository currently has these credentials exposed in committed files:

1. **OpenAI API Key**: `sk-proj--7b7htd-4NPNc...`
   - **Location**: `appsettings.json` and `appsettings.Production.json`
   - **Action Required**: Revoke and regenerate at https://platform.openai.com/api-keys

2. **Gmail App Password**: `cqkn cauj uvsg hzjm`
   - **Location**: `appsettings.json`
   - **Action Required**: Revoke and regenerate at https://myaccount.google.com/apppasswords

3. **JWT Secret**: Weak key visible in `appsettings.Production.json`
   - **Action Required**: Generate a strong 256-bit key

4. **Database Password**: `123PgSql`
   - **Location**: Multiple files
   - **Action Required**: Use strong password in production

## ? `.gitignore` Updated

The `.gitignore` has been updated to prevent committing sensitive files:

```gitignore
# Environment files with secrets
.env
.env.local
.env.production
.env.*.local

# Keep only example files
!.env.production.example
```

## ?? Files Overview

### **Created:**
- ? `.env` - Production environment file with current values
- ? `.env.production.example` - Template without secrets (safe to commit)

### **Updated:**
- ? `.gitignore` - Added .env files protection

### **Existing (Contains Secrets):**
- ?? `appsettings.json` - Contains email password and OpenAI key
- ?? `appsettings.Production.json` - Contains OpenAI key and weak JWT secret

## ?? Usage

### **Development (Local)**
Use `appsettings.Development.json` or your local `.env`

### **Production (Server)**

1. **Copy `.env` to server:**
```sh
scp .env user@hunter.geminicode.com.br:~/projects/hunter/.env
```

2. **Set proper permissions:**
```sh
ssh user@hunter.geminicode.com.br
cd ~/projects/hunter
chmod 600 .env
```

3. **Verify environment variables:**
```sh
cat .env | grep -v PASSWORD | grep -v KEY
```

## ?? Recommended Actions

### **1. Immediate: Revoke Exposed Secrets**

```sh
# 1. OpenAI API Key
# Visit: https://platform.openai.com/api-keys
# Revoke: sk-proj--7b7htd-4NPNc...
# Create new key

# 2. Gmail App Password
# Visit: https://myaccount.google.com/apppasswords
# Revoke: Hunter API password
# Create new app password

# 3. JWT Secret (Generate new 256-bit key)
pwsh -Command "[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))"

# Or use online tool:
# https://generate-secret.vercel.app/32
```

### **2. Update `.env` with New Secrets**

```sh
nano .env

# Update these lines:
JWT_SECRET_KEY=<new-256-bit-key>
OPENAI_API_KEY=<new-key>
EMAIL_PASSWORD=<new-app-password>
POSTGRES_PASSWORD=<strong-password>
```

### **3. Update GitHub Secrets**

Navigate to: `https://github.com/vanderlan/Hunter/settings/secrets/actions`

Update:
- `JWT_SECRET_KEY`
- `OPENAI_API_KEY`
- `EMAIL_PASSWORD`
- `POSTGRES_PASSWORD`

### **4. Remove Secrets from Git History (Optional)**

```sh
# WARNING: This rewrites git history
git filter-branch --force --index-filter \
  "git rm --cached --ignore-unmatch src/GeminiCode.Hunter.Api/appsettings.json" \
  --prune-empty --tag-name-filter cat -- --all

git push origin --force --all
```

## ?? Environment Variables in `.env`

| Category | Variables | Source |
|----------|-----------|--------|
| **Database** | POSTGRES_DB, POSTGRES_USER, POSTGRES_PASSWORD | appsettings.Production.json |
| **JWT** | JWT_SECRET_KEY, JWT_ISSUER, JWT_AUDIENCE, JWT_EXPIRATION_MINUTES | appsettings.Production.json |
| **OpenAI** | OPENAI_API_KEY | appsettings.json |
| **Email** | EMAIL_SMTP_HOST, EMAIL_SMTP_PORT, EMAIL_FROM, EMAIL_TO, EMAIL_PASSWORD, EMAIL_ENABLE_SSL | appsettings.json |
| **Elasticsearch** | ELASTICSEARCH_URI | appsettings.Production.json |
| **Serilog** | SERILOG_APP_NAME, SERILOG_INDEX_FORMAT, SERILOG_MIN_LEVEL | appsettings.json |
| **Application** | ASPNETCORE_ENVIRONMENT, ALLOWED_HOSTS | appsettings.Production.json |
| **Job Scraper** | JOB_SCRAPER_URL | appsettings.json |
| **Features** | FEATURE_AUTO_CANDIDATE_MATCHING | appsettings.json |
| **Logging** | LOGGING_LEVEL_DEFAULT, LOGGING_LEVEL_ASPNETCORE, LOGGING_LEVEL_BACKGROUND_SERVICES | appsettings.json |

## ?? Using in docker-compose.prod.yml

The `docker-compose.prod.yml` is already configured to use these environment variables:

```yaml
environment:
  - ConnectionStrings__PostgreSql=Host=postgresql;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
  - JwtOptions__SymmetricSecurityKey=${JWT_SECRET_KEY}
  - OpenAI__ApiKey=${OPENAI_API_KEY}
  # ... etc
```

## ? Verification Checklist

- [x] `.env` file created with current values
- [x] `.gitignore` updated to ignore `.env` files
- [x] `.env.production.example` available as template
- [ ] Revoke exposed OpenAI API key
- [ ] Revoke exposed Gmail app password
- [ ] Generate new strong JWT secret
- [ ] Update `.env` with new secrets
- [ ] Update GitHub secrets
- [ ] Copy `.env` to production server
- [ ] Set `.env` permissions to 600
- [ ] Test deployment

## ?? Next Steps

1. **Revoke and regenerate all exposed secrets**
2. **Update `.env` file with new values**
3. **Update GitHub secrets**
4. **Deploy to production:**
   ```sh
   git push origin master
   ```

5. **Verify deployment:**
   ```sh
   curl https://hunter.geminicode.com.br/api/health
   ```

## ?? Support

If you need help generating secure secrets:
- JWT Secret: Use PowerShell command above or online tool
- OpenAI Key: https://platform.openai.com/api-keys
- Gmail App Password: https://myaccount.google.com/apppasswords
- Strong Passwords: https://passwordsgenerator.net/

---

**Status**: ? `.env` file created and `.gitignore` updated  
**Security**: ?? Action required - revoke exposed credentials  
**Next**: Update secrets and deploy
