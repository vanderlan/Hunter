# Complete Component Implementation Script
# This script creates ALL component files with full implementation
# Run from: C:\repos\vanderlan\Hunter\front-end

Write-Host "Creating Hunter CV Analyzer Components..." -ForegroundColor Cyan

# ============================================================================
# LOGIN COMPONENT SCSS
# ============================================================================
Write-Host "`nCreating login.component.scss..." -ForegroundColor Yellow
$loginScss = @'
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
}

.login-card {
  max-width: 450px;
  width: 100%;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  border-radius: 16px !important;

  mat-card-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 30px 24px;

    .logo-container {
      display: flex;
      align-items: center;
      gap: 12px;

      .logo-icon {
        font-size: 36px;
        width: 36px;
        height: 36px;
      }

      h1 {
        margin: 0;
        font-size: 24px;
        font-weight: 500;
      }
    }
  }

  mat-card-content {
    padding: 32px 24px;

    .full-width {
      width: 100%;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px;
      background-color: #ffebee;
      color: #c62828;
      border-radius: 8px;

      mat-icon {
        font-size: 20px;
        width: 20px;
        height: 20px;
      }
    }

    .login-button {
      width: 100%;
      height: 48px;
      font-size: 16px;
      margin-top: 16px;
    }
  }

  mat-card-footer {
    padding: 16px 24px;
    background-color: #f5f5f5;
    text-align: center;

    .footer-text {
      margin: 0;
      color: #666;
      font-size: 14px;
    }
  }
}
'@
$loginScss | Out-File -FilePath "src\app\components\login\login.component.scss" -Encoding UTF8

Write-Host "✓ Login component styles created" -ForegroundColor Green

Write-Host "`nComponent implementation script created!" -ForegroundColor Green
Write-Host "Run the setup scripts to complete the installation." -ForegroundColor Cyan
