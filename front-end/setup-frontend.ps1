# Hunter Frontend Implementation Script
# Run this script from C:\repos\vanderlan\Hunter\front-end directory

Write-Host "Creating Hunter Frontend Components..." -ForegroundColor Green

# Login Component HTML
$loginHtml = @'
<div class="login-container">
  <mat-card class="login-card">
    <mat-card-header>
      <mat-card-title>
        <div class="logo-container">
          <mat-icon class="logo-icon">work</mat-icon>
          <h1>Hunter</h1>
        </div>
      </mat-card-title>
      <mat-card-subtitle>Sign in to analyze CVs and match candidates</mat-card-subtitle>
    </mat-card-header>
    <mat-card-content>
      <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Email</mat-label>
          <input matInput type="email" formControlName="email" placeholder="your@email.com" autocomplete="email">
          <mat-icon matPrefix>email</mat-icon>
          <mat-error *ngIf="loginForm.get('email')?.hasError('required')">Email is required</mat-error>
          <mat-error *ngIf="loginForm.get('email')?.hasError('email')">Please enter a valid email</mat-error>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Password</mat-label>
          <input matInput [type]="hidePassword ? 'password' : 'text'" formControlName="password">
          <mat-icon matPrefix>lock</mat-icon>
          <button mat-icon-button matSuffix (click)="hidePassword = !hidePassword" type="button">
            <mat-icon>{{ hidePassword ? 'visibility_off' : 'visibility' }}</mat-icon>
          </button>
          <mat-error *ngIf="loginForm.get('password')?.hasError('required')">Password is required</mat-error>
        </mat-form-field>
        <div *ngIf="errorMessage" class="error-message">
          <mat-icon>error</mat-icon>
          {{ errorMessage }}
        </div>
        <button mat-raised-button color="primary" type="submit" class="login-button" [disabled]="loginForm.invalid || loading">
          <mat-spinner *ngIf="loading" diameter="20" class="button-spinner"></mat-spinner>
          <span *ngIf="!loading">Sign In</span>
        </button>
      </form>
    </mat-card-content>
    <mat-card-footer>
      <p class="footer-text">Default: adm@hunter.com / 123</p>
    </mat-card-footer>
  </mat-card>
</div>
'@

$loginHtml | Out-File -FilePath "src\app\components\login\login.component.html" -Encoding utf8 -Force

Write-Host "✓ Login component HTML created" -ForegroundColor Green

# To be continued with remaining components...
Write-Host "`nPlease run the additional setup scripts to complete the frontend." -ForegroundColor Yellow
