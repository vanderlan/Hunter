# Hunter Frontend - Complete Setup Script
# Run this from: C:\repos\vanderlan\Hunter\front-end

Write-Host "Setting up Hunter Frontend..." -ForegroundColor Cyan

# App Routes
Write-Host "`nCreating app.routes.ts..." -ForegroundColor Yellow
$appRoutes = @'
import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./components/cv-analyzer/cv-analyzer.component').then(m => m.CvAnalyzerComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'jobs',
    loadComponent: () => import('./components/job-list/job-list.component').then(m => m.JobListComponent),
    canActivate: [AuthGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
'@
$appRoutes | Out-File -FilePath "src\app\app.routes.ts" -Encoding UTF8

# App Config
Write-Host "Creating app.config.ts..." -ForegroundColor Yellow
$appConfig = @'
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { routes } from './app.routes';
import { AuthInterceptor } from './interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideAnimations(),
    provideHttpClient(withInterceptorsFromDi()),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    importProvidersFrom(MatSnackBarModule)
  ]
};
'@
$appConfig | Out-File -FilePath "src\app\app.config.ts" -Encoding UTF8

Write-Host "`n✓ Core configuration files created" -ForegroundColor Green
Write-Host "`nNext: Run 'npm start' to begin development" -ForegroundColor Cyan
